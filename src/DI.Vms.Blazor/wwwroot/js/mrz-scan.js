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

/*  Which model the engine reads with.
 *
 *  'eng' is Tesseract's general English model: trained on proportional print, carrying a
 *  dictionary and every Latin letter in both cases. None of that helps here and all of it
 *  costs time, because the zone is one font with 37 characters in it.
 *
 *  A model trained on OCR-B alone is both smaller and better at exactly this, so the name is
 *  configuration rather than a constant - see DigitalCard:Language. The model file has to be
 *  reachable at TrainedDataUrl under that name for it to be worth setting. */
async function engine(engineBaseUrl, trainedDataUrl, language) {
    if (worker) return worker;

    /*  A failed load must not be remembered.
     *
     *  `loading` held the promise whatever became of it, so one bad moment - a CDN that did
     *  not answer, a tab woken on a dead network - left a rejected promise that every later
     *  call returned. The scanner was then broken until the page was reloaded, and nothing on
     *  screen said why. It is cleared on failure so pressing the button again means something.
     */
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

        const created = await window.Tesseract.createWorker(language || 'eng', 1, {
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
    })().catch(e => { loading = null; throw e; });

    return loading;
}

/*  Starts the engine loading before there is anything to read.
 *
 *  Called when the scan panel opens, so the several megabytes of recogniser are on their way
 *  down while the officer is still reaching for the card. Without it the first frame pays for
 *  the download, and the first read of a session looks like the slow one it is not.
 *
 *  Deliberately not awaited by the caller: it either finishes before the first frame, in which
 *  case nothing waited, or it does not, in which case the first frame waits on the same
 *  promise it would have started itself.
 */
export function warmUp(engineBaseUrl, trainedDataUrl, language) {
    engine(engineBaseUrl, trainedDataUrl, language).catch(() => {
        /* Nothing to report: there is no scan in progress to fail. The next real read asks
           again and its failure is the one the officer is shown. */
    });
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
async function scan(file, engineBaseUrl, trainedDataUrl, language) {
    const tess = await engine(engineBaseUrl, trainedDataUrl, language);
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

/**
 * The entry point the page calls: reads the chosen file straight out of the input.
 *
 * The file is never handed to .NET and back. A photograph is megabytes, and moving it
 * across the circuit twice - once to be scanned, once to be stored - would put that on a
 * tablet's wifi for no gain. What crosses is the recognised text, and one small JPEG.
 */
export async function scanFromInput(inputId, engineBaseUrl, trainedDataUrl, maximumBytes, language) {
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
        const results = await scan(file, engineBaseUrl, trainedDataUrl, language);
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

/* ---- reading from the live picture ------------------------------------------------
 *
 *  No shutter. The officer holds the card up and the frames are read as they arrive, which
 *  is how every document scanner behaves and how this should have behaved from the start.
 *
 *  Speed is the whole design here, so almost everything the still-photograph path does is
 *  dropped: one threshold rather than three, one orientation rather than four, and the guide
 *  box rather than the frame - which is a few hundred pixels instead of a megapixel and is
 *  where nearly all of the time went. The passes that were tried in turn are now replaced by
 *  simply reading the next frame, which arrives anyway.
 *
 *  Nothing here decides whether a read is good. Each frame's text goes to the server, which
 *  checks the digits and says whether to stop. That round trip is milliseconds against a
 *  recognition measured in hundreds, and it keeps the rule that protects a visitor record
 *  out of a script.
 */
let liveStop = null;

export async function startLive(videoId, dotnet, engineBaseUrl, trainedDataUrl, language) {
    const video = document.getElementById(videoId);
    if (!video) return { ok: false, problem: 'The camera panel is not on screen.' };

    let tess;
    try {
        tess = await engine(engineBaseUrl, trainedDataUrl, language);
    } catch (e) {
        return { ok: false, problem: e?.message ?? 'The text recogniser could not be loaded.' };
    }

    let stopped = false;
    liveStop = () => { stopped = true; };

    (async () => {
        let passes = 0;

        while (!stopped) {
            if (!video.videoWidth) { await wait(120); continue; }

            const guide = guideFor(video);
            showGuide(video, guide);

            const pass = passFor(passes);
            const canvas = guideRegion(video, guide, pass);

            let text = '';
            try {
                ({ data: { text } } = await tess.recognize(canvas));
            } catch {
                await wait(200);
                continue;
            }

            if (stopped) break;

            let good;
            try {
                good = await dotnet.invokeMethodAsync('OnFrameText', text ?? '');

                if (good) {
                    /* The frame that worked is the one kept, not a fresh grab: by the time a
                       second picture is taken the card has moved. */
                    await dotnet.invokeMethodAsync('OnFrameImage', snapshot(video, guide, pass.rotate));
                    return;
                }
            } catch {
                /* The circuit went. Nothing is listening, so stop reading and let go of the
                   camera rather than leaving the light on over a page that is gone. */
                closeCamera(videoId);
                return;
            }

            passes++;

            /* A breath for the page between frames. Recognition itself is in a worker, so
               this is not where the time goes - it is what keeps the preview smooth. */
            await wait(30);
        }
    })();

    return { ok: true };
}

export function stopLive() {
    if (liveStop) liveStop();
    liveStop = null;
}

const wait = ms => new Promise(r => setTimeout(r, ms));

/*  The guide box, thresholded, at the size the recogniser wants.
 *
 *  Only what is inside the on-screen rectangle is read. That is what makes a pass fast, and
 *  it is also what makes the result predictable: the officer can see exactly what is being
 *  looked at, so "it is not reading" becomes "move the card into the box".
 */
/*  The guide box is the shape of the card, worked out from the frame rather than fixed.
 *
 *  A fixed rectangle in percentages was wrong and wrong in the worst way: at 16:9 it came out
 *  half again wider than an ID card, so an officer who filled it width-wise pushed the bottom
 *  of the card - which is where the zone is - outside the region being read. The box has to be
 *  ID-1 shaped, and a frame may be 16:9 or 4:3, so it is computed.
 *
 *  The overlay on screen is positioned from this same rectangle, so the box the officer aims
 *  at and the pixels the recogniser sees cannot drift apart.
 */
const CARD_RATIO = 85.6 / 54;   // ID-1, the shape of every Emirates ID
const FILL = 0.88;              // a margin, so the card's edges stay visible inside the frame

function guideFor(video) {
    const vw = video.videoWidth;
    const vh = video.videoHeight;

    let w = vw * FILL;
    let h = w / CARD_RATIO;

    if (h > vh * FILL) {
        h = vh * FILL;
        w = h * CARD_RATIO;
    }

    return { x: (vw - w) / 2 / vw, y: (vh - h) / 2 / vh, w: w / vw, h: h / vh };
}

function showGuide(video, guide) {
    const box = video.parentElement?.querySelector('.scan-guide');
    if (!box) return;

    box.style.left = `${guide.x * 100}%`;
    box.style.top = `${guide.y * 100}%`;
    box.style.width = `${guide.w * 100}%`;
    box.style.height = `${guide.h * 100}%`;
}

/*  What one pass looks at.
 *
 *  The band - the bottom of the card, where the zone is - and not the whole card, which is
 *  the mistake this replaces. The zone is under a third of the card's height, so reading the
 *  whole card spends most of its pixels and most of its time on the photograph, the emblem,
 *  the Arabic and the notice about returning the card to a police station, and leaves the
 *  zone itself a fraction of the resolution. The symptom is precise, and was exactly what
 *  came back from the desk: the ID number reads, because it is also printed large and sits
 *  in line one, and the name, dates and nationality never do.
 *
 *  A whole-card pass is still worth having, because the number printed on the FRONT is only
 *  found that way - so one pass in three is the whole card. Upside down is not tried until
 *  several frames have failed: a card held the wrong way up is the rarer case, and trying
 *  both from the start halves the rate for everybody.
 */
const PASSES = [
    { band: true,  rotate: false },
    { band: true,  rotate: false },
    { band: false, rotate: false },
    { band: true,  rotate: true  },
    { band: true,  rotate: true  },
    { band: false, rotate: true  },
];

const UPRIGHT_ONLY = 3;     // the first three entries are the right way up
const BEFORE_ROTATING = 6;  // frames spent upright before the other way up joins the cycle

const passFor = n =>
    n < BEFORE_ROTATING ? PASSES[n % UPRIGHT_ONLY] : PASSES[n % PASSES.length];

/*  How much of the card's height the zone occupies, from the edge it sits against.
 *  Generous - the three lines are nearer a third - because a card held at a slight angle
 *  puts one corner of the zone higher than the other. */
const BAND = 0.45;

/*  Rendered widths, both expressed as the width of the whole card.
 *
 *  1100 across the card puts an MRZ character about 40 pixels tall, comfortably what the
 *  recogniser wants, while the band being under half the card's height keeps the canvas at
 *  about a third of a megapixel - fewer pixels than the whole card at 900, so this is faster
 *  as well as more accurate. 760 is enough for the front, whose ID number is printed several
 *  times larger than anything in the zone.
 */
const BAND_WIDTH = 1100;
const CARD_WIDTH = 760;

function sourceRect(video, guide, pass) {
    const x = video.videoWidth * guide.x;
    const w = video.videoWidth * guide.w;
    const y = video.videoHeight * guide.y;
    const h = video.videoHeight * guide.h;

    if (!pass.band) return { x, y, w, h };

    /* Held the other way up, the zone is against the top edge rather than the bottom. */
    const band = h * BAND;
    return { x, y: pass.rotate ? y : y + h - band, w, h: band };
}

function guideRegion(video, guide, pass) {
    const src = sourceRect(video, guide, pass);
    const cardWidth = video.videoWidth * guide.w;

    const width = Math.min(pass.band ? BAND_WIDTH : CARD_WIDTH, Math.round(cardWidth));
    const height = Math.max(1, Math.round(src.h * (width / src.w)));

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;

    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    ctx.imageSmoothingQuality = 'high';
    ctx.save();
    if (pass.rotate) { ctx.translate(width, height); ctx.rotate(Math.PI); }
    ctx.drawImage(video, src.x, src.y, src.w, src.h, 0, 0, width, height);
    ctx.restore();

    const pixels = ctx.getImageData(0, 0, width, height);
    const d = pixels.data;
    const grey = new Uint8ClampedArray(d.length / 4);

    for (let i = 0, g = 0; i < d.length; i += 4, g++) {
        grey[g] = (d[i] * 0.299 + d[i + 1] * 0.587 + d[i + 2] * 0.114) | 0;
    }

    /*  The local threshold, and only that one.
     *
     *  It is the one that copes with whatever light the desk has, and trying the other two
     *  would triple the time for the frames where this one would have worked anyway - and
     *  another frame is along in a moment regardless.
     *
     *  Thresholding the band alone is safe here in a way it would not be for Otsu: a global
     *  threshold taken from the strip has only ink and paper to look at and no idea what the
     *  card's paper is, which is why the still path crops after thresholding. This one is
     *  computed per window and sees both wherever it looks.
     */
    const binary = localThreshold(grey, width, height);

    for (let i = 0, g = 0; i < d.length; i += 4, g++) {
        d[i] = d[i + 1] = d[i + 2] = binary[g];
    }

    ctx.putImageData(pixels, 0, 0);
    return canvas;
}

/*  The guide box in colour: the picture kept with the visit.
 *
 *  The same rectangle that was read, not the whole frame - so the card fills the stored
 *  picture rather than sitting in the middle of a desk. That keeps it small, and it is also
 *  what lets faceFrom find the portrait, which is placed as a fraction of the card.
 */
function snapshot(video, guide, rotate) {
    const sx = video.videoWidth * guide.x;
    const sy = video.videoHeight * guide.y;
    const sw = video.videoWidth * guide.w;
    const sh = video.videoHeight * guide.h;

    const width = Math.min(1100, Math.round(sw));
    const height = Math.round(sh * (width / sw));

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;

    const ctx = canvas.getContext('2d');
    ctx.save();
    if (rotate) { ctx.translate(width, height); ctx.rotate(Math.PI); }
    ctx.drawImage(video, sx, sy, sw, sh, 0, 0, width, height);
    ctx.restore();

    return canvas.toDataURL('image/jpeg', 0.75).split(',')[1];
}

/*  There is deliberately no release().
 *
 *  There was one, called when the page was left, and it was wrong. The worker belongs to the
 *  browser page rather than to the component, and leaving this screen in a Blazor application
 *  is not leaving the page - so a reception desk that reads a card, looks at the report and
 *  comes back paid several seconds to build the recogniser again every single time.
 *
 *  What it bought was a few megabytes back while the officer was on another screen of the
 *  same application, which is not worth a visitor standing at the desk. The page going away
 *  frees it anyway, and that is the only moment it is genuinely not wanted.
 */
