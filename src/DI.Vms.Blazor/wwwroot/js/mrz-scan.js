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

/*  Otsu's threshold: the split between ink and paper that this image actually has.
 *
 *  This replaces a pair of hard-coded constants, and the constants were a real bug. They
 *  were tuned on one photograph; a brighter one put almost every pixel above the upper
 *  bound, so the whole card went white and the zone was erased before the recogniser saw
 *  it. The same card read one minute and not the next, which looked like bad luck and was
 *  bad code.
 *
 *  Otsu picks the threshold that best separates the histogram into two groups, per image.
 *  Twenty lines, no tuning, and it cannot be wrong about an exposure it was not written for.
 */
function otsuThreshold(histogram, total) {
    let sum = 0;
    for (let i = 0; i < 256; i++) sum += i * histogram[i];

    let sumBackground = 0;
    let weightBackground = 0;
    let best = 0;
    let bestVariance = -1;

    for (let t = 0; t < 256; t++) {
        weightBackground += histogram[t];
        if (weightBackground === 0) continue;

        const weightForeground = total - weightBackground;
        if (weightForeground === 0) break;

        sumBackground += t * histogram[t];

        const meanBackground = sumBackground / weightBackground;
        const meanForeground = (sum - sumBackground) / weightForeground;
        const between = weightBackground * weightForeground
            * (meanBackground - meanForeground) * (meanBackground - meanForeground);

        if (between > bestVariance) { bestVariance = between; best = t; }
    }

    return best;
}

/*  Bradley's adaptive threshold: every pixel judged against its own neighbourhood.
 *
 *  This is the one that answers "low light and bright light and glare". A global threshold -
 *  Otsu included - picks one number for the whole image, so a card with a window reflection
 *  on one half and shadow on the other has no single number that works: whichever is chosen,
 *  one half turns solid. Judging each pixel against the mean of the box around it removes
 *  the lighting from the problem entirely, because a dark character on dark paper is still
 *  darker than the paper beside it.
 *
 *  The integral image is what makes it cheap: the mean of any box is four array lookups,
 *  so the whole pass is linear however wide the window. That is why this can be the first
 *  thing tried rather than a fallback.
 */
function localThreshold(grey, width, height) {
    const integral = new Float64Array((width + 1) * (height + 1));

    for (let y = 0; y < height; y++) {
        let row = 0;
        for (let x = 0; x < width; x++) {
            row += grey[y * width + x];
            integral[(y + 1) * (width + 1) + (x + 1)] =
                integral[y * (width + 1) + (x + 1)] + row;
        }
    }

    /* A window about an eighth of the width: wide enough to hold several characters and
       their paper, narrow enough that a gradient across the card does not sit inside it. */
    const radius = Math.max(8, Math.round(width / 16));
    const out = new Uint8ClampedArray(grey.length);

    for (let y = 0; y < height; y++) {
        const y0 = Math.max(0, y - radius);
        const y1 = Math.min(height - 1, y + radius);

        for (let x = 0; x < width; x++) {
            const x0 = Math.max(0, x - radius);
            const x1 = Math.min(width - 1, x + radius);

            const count = (x1 - x0 + 1) * (y1 - y0 + 1);
            const sum = integral[(y1 + 1) * (width + 1) + (x1 + 1)]
                - integral[y0 * (width + 1) + (x1 + 1)]
                - integral[(y1 + 1) * (width + 1) + x0]
                + integral[y0 * (width + 1) + x0];

            /* Ink only when meaningfully darker than its surroundings. The 0.86 is what
               stops clean paper being read as a field of noise: without a margin, half of
               an even background falls on each side of its own mean. */
            out[y * width + x] = grey[y * width + x] * count < sum * 0.86 ? 0 : 255;
        }
    }

    return out;
}

/*  `mode` is 'local', 'otsu' or 'grey'.
 *
 *  Both are tried, because Tesseract binarises internally and does it well - so a hard
 *  threshold here sometimes destroys more than it removes, particularly on a photograph of
 *  a screen where the moiré is what gets sharpened into false strokes. Which of the two
 *  wins is decided by the check digits rather than by an opinion held here.
 */
function draw(source, { flip, rotate }, width, height, mode, band) {
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

    const pixels = ctx.getImageData(0, 0, width, height);
    const d = pixels.data;

    const histogram = new Uint32Array(256);
    const grey = new Uint8ClampedArray(d.length / 4);

    for (let i = 0, g = 0; i < d.length; i += 4, g++) {
        const value = (d[i] * 0.299 + d[i + 1] * 0.587 + d[i + 2] * 0.114) | 0;
        grey[g] = value;
        histogram[value]++;
    }

    if (mode === 'grey') {
        for (let i = 0, g = 0; i < d.length; i += 4, g++) {
            d[i] = d[i + 1] = d[i + 2] = grey[g];
        }
    } else if (mode === 'local') {
        const binary = localThreshold(grey, width, height);
        for (let i = 0, g = 0; i < d.length; i += 4, g++) {
            d[i] = d[i + 1] = d[i + 2] = binary[g];
        }
    } else {
        const t = otsuThreshold(histogram, grey.length);

        /* A margin either side of the threshold rather than a cliff, so a character's
           anti-aliased edge keeps its shape instead of being gnawed at. */
        const low = Math.max(0, t - 18);
        const high = Math.min(255, t + 18);
        const span = Math.max(1, high - low);

        for (let i = 0, g = 0; i < d.length; i += 4, g++) {
            const v = grey[g] <= low ? 0 : grey[g] >= high ? 255 : ((grey[g] - low) * 255 / span) | 0;
            d[i] = d[i + 1] = d[i + 2] = v;
        }
    }

    ctx.putImageData(pixels, 0, 0);

    if (!band) return canvas;

    /* Cropped after thresholding, not before: a threshold computed from the strip alone has
       only ink and paper to look at and no idea what the card's paper is. */
    const strip = document.createElement('canvas');
    const top = Math.round(height * 0.55);
    strip.width = width * 2;
    strip.height = (height - top) * 2;

    const stripCtx = strip.getContext('2d');
    stripCtx.imageSmoothingEnabled = true;
    stripCtx.imageSmoothingQuality = 'high';
    stripCtx.drawImage(canvas, 0, top, width, height - top, 0, 0, strip.width, strip.height);

    return strip;
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

    /*  Ordered by what usually wins, because the caller stops at the first read whose check
     *  digits hold - so the common case costs one pass, not all of them.
     *
     *  The band first: the zone is a tenth of the card's height, so on the whole card each
     *  character is a handful of pixels, and that is where a chevron gets dropped or a line
     *  read twice. Cropped and doubled it is four times the area for a fraction of the work.
     *
     *  Then the thresholds in order of how much light they forgive. Only if all of that
     *  fails is the whole card tried, in case the zone was not where the crop assumed. */
    const attempts = [];

    for (const mode of ['local', 'otsu', 'grey']) {
        for (const orientation of ORIENTATIONS) {
            attempts.push({ orientation, mode, band: true });
        }
    }

    for (const orientation of ORIENTATIONS) {
        attempts.push({ orientation, mode: 'local', band: false });
    }

    for (const { orientation, mode, band } of attempts) {
        const whole = draw(bitmap, orientation, width, height, mode, band);
        const { data } = await tess.recognize(whole);

        results.push({
            orientation: `${orientation.name}, ${mode}, ${band ? 'band' : 'whole'}`,
            text: data.text ?? '',
        });
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

/*  The portrait, cut out of the front of the card.
 *
 *  An Emirates ID puts the photograph in the same place on every card, so the crop is a
 *  proportion of the card rather than anything found in the image. That is the whole of the
 *  method, and it has one condition: the card has to fill the frame. Background around it
 *  shifts the crop and the officer gets a picture of a desk.
 *
 *  Which is why the result is shown before it is kept. Detecting the card's edges would
 *  remove the condition and needs an image-processing library this deployment has no way to
 *  fetch; showing the officer what will be stored costs nothing and fails visibly.
 *
 *  320 pixels wide, because this is a face on a report and beside a visitor at a desk - not
 *  an archive. That lands at about the size of the JPEG the chip returns, which is the
 *  budget this has to live inside.
 */
const PORTRAIT = { x0: 0.075, x1: 0.265, y0: 0.255, y1: 0.735 };

export async function faceFrom(frontBase64) {
    if (!frontBase64) return null;

    try {
        const bitmap = await createImageBitmap(
            await (await fetch(`data:image/jpeg;base64,${frontBase64}`)).blob());

        const sx = bitmap.width * PORTRAIT.x0;
        const sy = bitmap.height * PORTRAIT.y0;
        const sw = bitmap.width * (PORTRAIT.x1 - PORTRAIT.x0);
        const sh = bitmap.height * (PORTRAIT.y1 - PORTRAIT.y0);

        const width = 320;
        const height = Math.round(sh * (width / sw));

        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;

        const ctx = canvas.getContext('2d');
        ctx.imageSmoothingQuality = 'high';
        ctx.drawImage(bitmap, sx, sy, sw, sh, 0, 0, width, height);

        return canvas.toDataURL('image/jpeg', 0.72).split(',')[1];
    } catch {
        // A face is a nicety; a check-in is not. Never the reason a visit cannot be recorded.
        return null;
    }
}

/** Frees the worker when the desk leaves the screen; it holds several megabytes. */
export async function release() {
    const w = worker;
    worker = null;
    loading = null;
    if (w) { try { await w.terminate(); } catch { /* going away anyway */ } }
}
