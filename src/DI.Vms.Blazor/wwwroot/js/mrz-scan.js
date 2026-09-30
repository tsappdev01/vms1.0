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

    const width = Math.min(1600, bitmap.width);
    const height = Math.round(bitmap.height * (width / bitmap.width));

    const results = [];

    for (const orientation of ORIENTATIONS) {
        const whole = draw(bitmap, orientation, width, height);

        /*  The strip, first and enlarged.
         *
         *  The zone is three lines across the bottom edge - perhaps a tenth of the card's
         *  height - so in a photograph of the whole card each character is a handful of
         *  pixels, and that is where a recogniser starts dropping a chevron or reading a
         *  line twice. Cropping to the band and doubling it gives the same characters four
         *  times the area, for a fraction of the work of the full image.
         *
         *  Tried before the whole card because the caller stops at the first read whose
         *  check digits hold, and this is the one that usually does. */
        const band = document.createElement('canvas');
        const bandTop = Math.round(whole.height * 0.55);
        band.width = whole.width * 2;
        band.height = (whole.height - bandTop) * 2;

        const ctx = band.getContext('2d');
        ctx.imageSmoothingEnabled = true;
        ctx.imageSmoothingQuality = 'high';
        ctx.drawImage(whole, 0, bandTop, whole.width, whole.height - bandTop,
            0, 0, band.width, band.height);

        for (const [what, canvas] of [['band', band], ['whole', whole]]) {
            const { data } = await tess.recognize(canvas);
            results.push({ orientation: `${orientation.name}, ${what}`, text: data.text ?? '' });
        }
    }

    return results;
}

/* ---- the camera ------------------------------------------------------------------
 *
 *  A live camera, because `capture="environment"` is a hint browsers honour on phones and
 *  ignore on desktops - which is why a PC showed a file picker where reception expected a
 *  viewfinder.
 *
 *  The preview is deliberately NOT mirrored. Browsers and laptop drivers mirror a selfie
 *  preview by convention, and that convention is wrong here: this is a document being held
 *  up, and text reads backwards. Nothing relies on getting it right, though - the recogniser
 *  tries all four orientations and the check digits decide - so this is about the officer
 *  being able to aim, not about correctness.
 */
const streams = new Map();

export async function openCamera(videoId) {
    const video = document.getElementById(videoId);
    if (!video) return { ok: false, problem: 'The camera panel is not on screen.' };

    try {
        const stream = await navigator.mediaDevices.getUserMedia({
            video: {
                /* The rear camera where there is one - a tablet on a desk stand has the
                   visitor's card in front of it, not the officer's face. Not "exact", so a
                   laptop with only a front camera still works. */
                facingMode: 'environment',
                width: { ideal: 1920 },
                height: { ideal: 1080 },
            },
            audio: false,
        });

        streams.set(videoId, stream);
        video.srcObject = stream;
        await video.play();
        return { ok: true };
    } catch (e) {
        const reason = e?.name === 'NotAllowedError'
            ? 'The browser blocked the camera. Allow it for this site and try again.'
            : e?.name === 'NotFoundError'
                ? 'This device has no camera. Choose a photograph instead.'
                : (e?.message ?? 'The camera could not be opened.');
        return { ok: false, problem: reason };
    }
}

export function closeCamera(videoId) {
    const stream = streams.get(videoId);
    if (stream) stream.getTracks().forEach(t => t.stop());
    streams.delete(videoId);

    const video = document.getElementById(videoId);
    if (video) video.srcObject = null;
}

/** One frame, full sensor resolution, as base64 JPEG. Not mirrored: a video frame never is. */
export function grab(videoId) {
    const video = document.getElementById(videoId);
    if (!video || !video.videoWidth) return null;

    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext('2d').drawImage(video, 0, 0);

    return canvas.toDataURL('image/jpeg', 0.92).split(',')[1];
}

/** A chosen file, as base64, so the camera and the picker feed the same code below. */
export async function fileAsBase64(inputId, maximumBytes) {
    const file = document.getElementById(inputId)?.files?.[0];
    if (!file) return null;
    if (file.size > maximumBytes) return { tooLarge: true };

    const bitmap = await bitmapOf(file);
    const width = Math.min(1920, bitmap.width);
    const height = Math.round(bitmap.height * (width / bitmap.width));

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    canvas.getContext('2d').drawImage(bitmap, 0, 0, width, height);

    return { image: canvas.toDataURL('image/jpeg', 0.92).split(',')[1] };
}

/** Recognises a base64 image in all four orientations. */
export async function scanImage(base64, engineBaseUrl, trainedDataUrl) {
    try {
        const blob = await (await fetch(`data:image/jpeg;base64,${base64}`)).blob();
        const results = await scan(blob, engineBaseUrl, trainedDataUrl);
        return { ok: true, results };
    } catch (e) {
        return { ok: false, problem: e?.message ?? 'The photograph could not be read.' };
    }
}

/**
 * The two sides as one picture, front above back.
 *
 * One image because the visit stores one, and because the pair is the evidence: the face
 * the officer was shown and the zone the details came from, in the same frame, neither able
 * to be separated from the other afterwards.
 */
export async function compose(frontBase64, backBase64) {
    const sides = [];
    for (const b of [frontBase64, backBase64]) {
        if (!b) continue;
        sides.push(await createImageBitmap(await (await fetch(`data:image/jpeg;base64,${b}`)).blob()));
    }
    if (sides.length === 0) return null;

    const width = Math.min(1100, Math.max(...sides.map(s => s.width)));
    const heights = sides.map(s => Math.round(s.height * (width / s.width)));
    const gap = sides.length > 1 ? 12 : 0;

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = heights.reduce((a, b) => a + b, 0) + gap;

    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    let y = 0;
    sides.forEach((s, i) => {
        ctx.drawImage(s, 0, y, width, heights[i]);
        y += heights[i] + gap;
    });

    return canvas.toDataURL('image/jpeg', 0.75).split(',')[1];
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
