/*  Reading the machine-readable zone off a photograph of an Emirates ID.
 *
 *  The recogniser is Tesseract, compiled to WebAssembly and run here in the browser. It
 *  costs nothing per read and needs nothing installed on the server, which is why it is
 *  here rather than at a cloud OCR endpoint.
 *
 *  Nothing in this file decides whether a read is good. It returns text; the server parses
 *  it and checks the digits, and a misread is refused there. That division is deliberate -
 *  the rule that protects a visitor record from a wrong ID number should not live in a
 *  script the page could be made to skip.
 */

let worker = null;
let loading = null;

/*  The zone is OCR-B: capitals, digits and the chevron, nothing else. Telling the engine so
 *  is the single biggest improvement available here - without it "0" and "O", "1" and "I",
 *  "5" and "S" are all in play on every character, and the check digits then reject reads
 *  that were nearly right. */
const ALPHABET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789<';

async function engine(engineBaseUrl, trainedDataUrl) {
    if (worker) return worker;

    loading ??= (async () => {
        if (!window.Tesseract) {
            await new Promise((resolve, reject) => {
                const tag = document.createElement('script');
                tag.src = `${engineBaseUrl}/tesseract.min.js`;
                tag.onload = resolve;
                tag.onerror = () => reject(new Error(
                    'The text recogniser could not be loaded. If this network blocks public ' +
                    'CDNs, host it with the application - see DigitalCard:EngineBaseUrl.'));
                document.head.appendChild(tag);
            });
        }

        const created = await window.Tesseract.createWorker('eng', 1, {
            workerPath: `${engineBaseUrl}/worker.min.js`,
            langPath: trainedDataUrl,
            /* The zone is one line of text per line of the zone, in a fixed block. Telling
               the engine not to hunt for page layout is both faster and more accurate. */
            legacyCore: false,
        });

        await created.setParameters({
            tessedit_char_whitelist: ALPHABET,
            tessedit_pageseg_mode: '6',   // a uniform block of text
            preserve_interword_spaces: '0',
        });

        worker = created;
        return created;
    })();

    return loading;
}

/*  The card as the camera saw it, and the three other ways it might have been.
 *
 *  A laptop webcam preview is conventionally mirrored, and whether the captured frame is
 *  mirrored too depends on the browser, the driver and the application - it is not
 *  something to rely on getting right. A card can also be held upside down, which nobody
 *  notices on a screen full of chevrons.
 *
 *  So rather than detect any of that, all four orientations are tried and the check digits
 *  on the server decide which one was real. A mirrored zone cannot pass them, so there is
 *  no risk of accepting the wrong one - only the cost of up to four passes, on a small
 *  greyscale image, which is cheaper than a visitor being asked to pose the card again.
 */
const ORIENTATIONS = [
    { name: 'as taken',            flip: false, rotate: false },
    { name: 'mirrored',            flip: true,  rotate: false },
    { name: 'upside down',         flip: false, rotate: true  },
    { name: 'mirrored, upside down', flip: true, rotate: true },
];

function draw(source, { flip, rotate }, width, height) {
    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;

    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    ctx.save();
    ctx.translate(flip ? width : 0, rotate ? height : 0);
    ctx.scale(flip ? -1 : 1, rotate ? -1 : 1);
    if (rotate && !flip) { ctx.translate(width, 0); ctx.scale(-1, 1); }
    ctx.drawImage(source, 0, 0, width, height);
    ctx.restore();

    /*  Greyscale and a hard contrast stretch. The zone is black on near-white, and a
     *  photograph of a phone screen arrives grey on grey with the moiré of one pixel grid
     *  seen through another. Pushing it to black and white removes most of that before the
     *  recogniser has to reason about it. */
    const pixels = ctx.getImageData(0, 0, width, height);
    const d = pixels.data;

    for (let i = 0; i < d.length; i += 4) {
        const grey = (d[i] * 0.299 + d[i + 1] * 0.587 + d[i + 2] * 0.114);
        const v = grey < 110 ? 0 : grey > 165 ? 255 : (grey - 110) * (255 / 55);
        d[i] = d[i + 1] = d[i + 2] = v;
    }

    ctx.putImageData(pixels, 0, 0);
    return canvas;
}

async function bitmapOf(file) {
    if (window.createImageBitmap) return await createImageBitmap(file);

    // Safari and older Edge: the long way round.
    const url = URL.createObjectURL(file);
    try {
        const img = new Image();
        await new Promise((ok, fail) => { img.onload = ok; img.onerror = fail; img.src = url; });
        return img;
    } finally {
        URL.revokeObjectURL(url);
    }
}

/**
 * Recognises the text in one image, in every orientation, and returns them all.
 *
 * All of them, not the best of them: which one is right is decided on the server by
 * arithmetic, and a "best" chosen here by a score would sometimes be the mirrored one.
 */
export async function scan(file, engineBaseUrl, trainedDataUrl) {
    const tess = await engine(engineBaseUrl, trainedDataUrl);
    const bitmap = await bitmapOf(file);

    /*  Scaled so the zone is around the size Tesseract was trained on. Bigger is not better
     *  here: a 12-megapixel photograph is slower and no more accurate than the same card at
     *  1600 pixels across. */
    const width = Math.min(1600, bitmap.width);
    const height = Math.round(bitmap.height * (width / bitmap.width));

    const results = [];

    for (const orientation of ORIENTATIONS) {
        const canvas = draw(bitmap, orientation, width, height);
        const { data } = await tess.recognize(canvas);
        results.push({ orientation: orientation.name, text: data.text ?? '' });
    }

    return results;
}

/**
 * The entry point the page calls: reads the chosen file straight out of the input.
 *
 * The file is never handed to .NET and back. A photograph is megabytes, and moving it
 * across the circuit twice - once to be scanned, once to be stored - would put that on a
 * tablet's wifi for no gain. What crosses is the recognised text, and one small JPEG.
 */
export async function scanFromInput(inputId, engineBaseUrl, trainedDataUrl, maximumBytes) {
    const input = document.getElementById(inputId);
    const file = input?.files?.[0];

    if (!file) return { ok: false, problem: 'No photograph was chosen.' };

    if (file.size > maximumBytes) {
        return {
            ok: false,
            problem: 'That file is too large to be a photograph of a card. '
                + 'Take a picture rather than choosing a video.',
        };
    }

    try {
        const results = await scan(file, engineBaseUrl, trainedDataUrl);
        return { ok: true, results, image: await thumbnail(file) };
    } catch (e) {
        return { ok: false, problem: e?.message ?? 'The photograph could not be read.' };
    }
}

/**
 * A small JPEG of what was photographed, kept with the visit.
 *
 * Downscaled deliberately: this is evidence of what the officer was shown, not a copy of
 * the visitor's identity document to archive at full resolution.
 */
async function thumbnail(file) {
    const bitmap = await bitmapOf(file);
    const width = Math.min(1100, bitmap.width);
    const height = Math.round(bitmap.height * (width / bitmap.width));

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    canvas.getContext('2d').drawImage(bitmap, 0, 0, width, height);

    return canvas.toDataURL('image/jpeg', 0.7).split(',')[1];
}

/** Frees the worker when the desk leaves the screen; it holds several megabytes. */
export async function release() {
    const w = worker;
    worker = null;
    loading = null;
    if (w) { try { await w.terminate(); } catch { /* going away anyway */ } }
}
