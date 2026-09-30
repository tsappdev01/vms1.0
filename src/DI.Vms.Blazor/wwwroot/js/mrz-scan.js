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

                /*  As many pixels as the device will give, because this is the binding
                 *  constraint and not a preference. The zone is thirty characters across
                 *  roughly a third of the frame's width when a card is held at a comfortable
                 *  distance; at 1280 that is fifteen pixels a character, which no recogniser
                 *  reads. At 2560 it is thirty, which is what Tesseract asks for.
                 *
                 *  `ideal` rather than `exact` throughout: a device that cannot manage this
                 *  gives its best instead of refusing, and a laptop webcam that tops out at
                 *  720p still works with the card brought closer. */
                width: { ideal: 3840 },
                height: { ideal: 2160 },
            },
            audio: false,
        });

        streams.set(videoId, stream);
        video.srcObject = stream;
        await video.play();

        const [track] = stream.getVideoTracks();
        const settings = track?.getSettings?.() ?? {};

        /*  Zoom where the hardware has it - a tablet on a stand cannot move closer to the
         *  card, so this is the only way it gets more pixels onto the zone. Not supported
         *  everywhere and never required: the constraint is applied on its own so a device
         *  that rejects it keeps the stream it already has. */
        try {
            const zoom = track?.getCapabilities?.().zoom;
            if (zoom) {
                await track.applyConstraints({
                    advanced: [{ zoom: Math.min(zoom.max, Math.max(zoom.min, 1.6)) }],
                });
            }
        } catch { /* no zoom, or refused - the picture is unaffected either way */ }

        return { ok: true, width: settings.width ?? 0, height: settings.height ?? 0 };
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

            /* The found zone where one can be found, and the fixed band where it cannot, so a
               frame this makes no sense of is no worse off than before there was a detector
               rather than being skipped entirely. */
            const found = pass.band ? zoneRegion(video, guide, pass.rotate ? Math.PI : 0) : null;
            const canvas = found?.canvas ?? guideRegion(video, guide, pass);

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
                /*  The size goes with the text, because the commonest reason this fails is
                 *  not the recognising - it is that the camera never resolved the characters.
                 *  A card held at arm's length on a 720p webcam puts an MRZ character at
                 *  about fifteen pixels, and nothing downstream can recover what was not
                 *  captured. Sent up so the screen can say "bring the card closer", which is
                 *  the one thing that actually fixes it. */
                good = await dotnet.invokeMethodAsync('OnFrameText', text ?? '',
                    Math.round(found?.pixelsPerCharacter ?? 0));

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

/* ---- finding the zone ---------------------------------------------------------------
 *
 *  Measured against rendered cards held the way the desk actually holds one - small in the
 *  frame, with a lit wall and a person behind them - and not against a card filling a clean
 *  box. The clean version scored 10/10 and the realistic one 0/8, and the difference was
 *  never the recogniser.
 *
 *  Two things settle where the zone is, and neither works alone:
 *
 *    Texture. Print has local variance; a wall, a face and a shirt have none, however bright
 *    they are. Brightness was tried first and fails exactly where the photograph from the
 *    desk fails, because the lit wall behind the shoulder is brighter than the card.
 *
 *    The row profile, inside what texture found. Texture alone takes the whole of the card's
 *    printing with it; the profile alone drowns in the room. Bounded by texture it is looking
 *    at print and nothing else, and the three lines are then the obvious thing in it.
 *
 *  The pure functions here take a greyscale array and no canvas, which is what let every
 *  claim above be a measurement rather than an opinion.
 */
const SCOUT_WIDTH = 480;
const ZONE_WIDTH = 1150;

/** The longest stretch where the projection stays above `need`. */
function longestRun(counts, need) {
    let bestStart = -1, bestLength = 0, start = -1;

    for (let i = 0; i < counts.length; i++) {
        if (counts[i] >= need) { if (start < 0) start = i; }
        else {
            if (start >= 0 && i - start > bestLength) { bestLength = i - start; bestStart = start; }
            start = -1;
        }
    }

    if (start >= 0 && counts.length - start > bestLength) {
        bestLength = counts.length - start;
        bestStart = start;
    }

    return bestLength ? [bestStart, bestStart + bestLength - 1] : null;
}

/*  Where the printing is: bright, and locally varied.
 *
 *  Local standard deviation by integral image, so a window costs four lookups whatever its
 *  size and the whole pass is linear. That is what keeps this in tens of milliseconds, which
 *  is what it has to be when it runs on every frame.
 */
export function printBounds(grey, width, height) {
    const histogram = new Uint32Array(256);
    for (const v of grey) histogram[v]++;
    const bright = otsuThreshold(histogram, grey.length) * 0.9;

    const stride = width + 1;
    const sums = new Float64Array(stride * (height + 1));
    const squares = new Float64Array(stride * (height + 1));

    for (let y = 0; y < height; y++) {
        let rowSum = 0, rowSquares = 0;
        for (let x = 0; x < width; x++) {
            const v = grey[y * width + x];
            rowSum += v; rowSquares += v * v;
            sums[(y + 1) * stride + x + 1] = sums[y * stride + x + 1] + rowSum;
            squares[(y + 1) * stride + x + 1] = squares[y * stride + x + 1] + rowSquares;
        }
    }

    const area = (a, x0, y0, x1, y1) =>
        a[(y1 + 1) * stride + x1 + 1] - a[y0 * stride + x1 + 1]
        - a[(y1 + 1) * stride + x0] + a[y0 * stride + x0];

    const radius = Math.max(2, Math.round(width / 120));
    const rows = new Int32Array(height), columns = new Int32Array(width);

    for (let y = 0; y < height; y++) {
        const y0 = Math.max(0, y - radius), y1 = Math.min(height - 1, y + radius);
        for (let x = 0; x < width; x++) {
            const x0 = Math.max(0, x - radius), x1 = Math.min(width - 1, x + radius);
            const count = (x1 - x0 + 1) * (y1 - y0 + 1);
            const mean = area(sums, x0, y0, x1, y1) / count;
            if (mean <= bright) continue;

            const variance = Math.max(0, area(squares, x0, y0, x1, y1) / count - mean * mean);
            if (Math.sqrt(variance) > 12) { rows[y]++; columns[x]++; }
        }
    }

    let rowPeak = 0, columnPeak = 0;
    for (const v of rows) if (v > rowPeak) rowPeak = v;
    for (const v of columns) if (v > columnPeak) columnPeak = v;
    if (!rowPeak || !columnPeak) return null;

    const down = longestRun(rows, rowPeak * 0.15);
    const across = longestRun(columns, columnPeak * 0.15);
    if (!down || !across) return null;

    return { x0: across[0], x1: across[1], y0: down[0], y1: down[1] };
}

/** Rows of ink grouped into lines, with the measurements a triple is judged on. */
export function linesIn(binary, width, height, within) {
    const ink = new Int32Array(height);
    const left = new Int32Array(height).fill(width);
    const right = new Int32Array(height).fill(-1);

    for (let y = within.y0; y <= within.y1; y++) {
        for (let x = within.x0; x <= within.x1; x++) {
            if (binary[y * width + x]) continue;
            ink[y]++;
            if (x < left[y]) left[y] = x;
            if (x > right[y]) right[y] = x;
        }
    }

    const span = within.x1 - within.x0 + 1;
    const lines = [];
    let start = -1;

    const close = end => {
        if (end - start < 1) return;
        let x0 = width, x1 = 0, total = 0;
        for (let y = start; y <= end; y++) {
            if (right[y] < 0) continue;
            x0 = Math.min(x0, left[y]); x1 = Math.max(x1, right[y]); total += ink[y];
        }
        const w = Math.max(1, x1 - x0 + 1);
        lines.push({
            y0: start, y1: end, h: end - start + 1, x0, x1, w,
            density: total / ((end - start + 1) * w),
        });
    };

    for (let y = within.y0; y <= within.y1; y++) {
        const inked = ink[y] > span * 0.02 && ink[y] < span * 0.70;
        if (inked) { if (start < 0) start = y; }
        else if (start >= 0) { close(y - 1); start = -1; }
    }
    if (start >= 0) close(within.y1);

    return lines;
}

/*  The three lines, judged as a set.
 *
 *  No single line is unmistakable; the set is. Three of the same height, the same width, the
 *  same distance apart, left edges aligned, and wider than anything else on the card. Nothing
 *  else printed on either side of a card has that shape.
 */
export function bestTriple(lines, span) {
    let best = null, bestScore = -1;

    for (let i = 0; i + 2 < lines.length; i++) {
        const t = [lines[i], lines[i + 1], lines[i + 2]];
        if (t.some(l => l.density < 0.20 || l.density > 0.75)) continue;

        const heights = t.map(l => l.h), widths = t.map(l => l.w);
        const meanHeight = (heights[0] + heights[1] + heights[2]) / 3;
        const meanWidth = (widths[0] + widths[1] + widths[2]) / 3;
        if (meanWidth < span * 0.25) continue;

        const gap1 = t[1].y0 - t[0].y1, gap2 = t[2].y0 - t[1].y1;
        if (gap1 < 0 || gap2 < 0 || gap1 > meanHeight * 1.6 || gap2 > meanHeight * 1.6) continue;

        const spread = a => Math.max(...a) / Math.max(1, Math.min(...a));
        const even = 1 / (1 + Math.abs(gap1 - gap2) / Math.max(1, meanHeight));
        const alike = 1 / spread(heights) * 1 / spread(widths);
        const aligned = 1 / (1 + (Math.max(...t.map(l => l.x0)) - Math.min(...t.map(l => l.x0)))
            / Math.max(1, meanWidth) * 4);

        const score = even * alike * aligned * (meanWidth / span);
        if (score > bestScore) { bestScore = score; best = t; }
    }

    if (!best) return null;

    const x0 = Math.min(...best.map(l => l.x0)), x1 = Math.max(...best.map(l => l.x1));
    const y0 = best[0].y0, y1 = best[2].y1;
    const padY = (y1 - y0) * 0.18, padX = (x1 - x0) * 0.04;

    return { x0: x0 - padX, x1: x1 + padX, y0: y0 - padY, y1: y1 + padY };
}

/*  How level the text is, as a number to be maximised.
 *
 *  With the lines level a row is either through a line or between two, and the ink per row
 *  swings hard between them; tilted, every row catches part of both and the profile flattens.
 *  Summing the squared step between neighbouring rows measures that swing, so the angle that
 *  maximises it is level - with no idea needed of what the characters are.
 */
export function skewScore(grey, width, height) {
    const histogram = new Uint32Array(256);
    for (const v of grey) histogram[v]++;

    const threshold = otsuThreshold(histogram, grey.length);
    const rows = new Float64Array(height);

    for (let y = 0; y < height; y++) {
        let n = 0;
        for (let x = 0; x < width; x++) if (grey[y * width + x] < threshold) n++;
        rows[y] = n;
    }

    let score = 0;
    for (let y = 1; y < height; y++) { const d = rows[y] - rows[y - 1]; score += d * d; }
    return score;
}

/* One canvas, reused: a loop that allocates one per frame spends more time being collected
   than recognising. */
const scratch = document.createElement('canvas');

/** A rectangle of the frame, levelled, as greyscale. Outside the frame reads as paper. */
function greyOf(video, rect, width, height, angle) {
    scratch.width = width;
    scratch.height = height;

    const ctx = scratch.getContext('2d', { willReadFrequently: true });
    ctx.imageSmoothingQuality = 'high';
    ctx.fillStyle = '#fff';
    ctx.fillRect(0, 0, width, height);

    ctx.save();
    ctx.translate(width / 2, height / 2);
    ctx.rotate(-angle);
    ctx.scale(width / rect.w, height / rect.h);
    ctx.translate(-(rect.x + rect.w / 2), -(rect.y + rect.h / 2));
    ctx.drawImage(video, 0, 0);
    ctx.restore();

    const d = ctx.getImageData(0, 0, width, height).data;
    const grey = new Uint8ClampedArray(width * height);

    for (let i = 0, g = 0; i < d.length; i += 4, g++) {
        grey[g] = (d[i] * 0.299 + d[i + 1] * 0.587 + d[i + 2] * 0.114) | 0;
    }

    return grey;
}

const boxOf = (video, guide) => ({
    x: video.videoWidth * guide.x, y: video.videoHeight * guide.y,
    w: video.videoWidth * guide.w, h: video.videoHeight * guide.h,
});

/** Where the zone is in the frame, at this assumed angle - or null. */
function findZone(video, guide, angle) {
    const box = boxOf(video, guide);
    const width = SCOUT_WIDTH;
    const height = Math.max(8, Math.round(box.h * (width / box.w)));

    const grey = greyOf(video, box, width, height, angle);
    const print = printBounds(grey, width, height);
    if (!print) return null;

    const zone = bestTriple(
        linesIn(localThreshold(grey, width, height), width, height, print),
        print.x1 - print.x0 + 1);

    const found = zone ?? print;
    const x0 = Math.max(0, found.x0), x1 = Math.min(width - 1, found.x1);
    const y0 = Math.max(0, found.y0), y1 = Math.min(height - 1, found.y1);
    if (x1 <= x0 || y1 <= y0) return null;

    const w = (x1 - x0) / width * box.w;
    const h = (y1 - y0) / height * box.h;

    /*  The zone was measured in a scout already turned by `angle`, so its position has to be
     *  turned back. Left out, this is invisible at zero - nothing rotates, so nothing moves -
     *  and at twenty degrees it slides the crop far enough sideways to cut the last character
     *  off every line, so the three lines look perfect on screen and read one short. */
    const boxCentreX = box.x + box.w / 2, boxCentreY = box.y + box.h / 2;
    const u = box.x + (x0 + x1) / 2 / width * box.w - boxCentreX;
    const v = box.y + (y0 + y1) / 2 / height * box.h - boxCentreY;
    const ca = Math.cos(angle), sa = Math.sin(angle);

    return {
        x: boxCentreX + u * ca - v * sa - w / 2,
        y: boxCentreY + u * sa + v * ca - h / 2,
        w, h,
    };
}

/*  Levelling, on the block alone.
 *
 *  A few hundred pixels rather than the whole box, and nine small draws rather than fifty. The
 *  wide sweep this replaces was most of a detection budget that had grown to several hundred
 *  milliseconds a frame; this is tens, and finds the same angle for anything a hand holds. A
 *  card lying on a desk at thirty degrees is beyond it, and that is the trade.
 */
function skewOf(video, rect, from = 0) {
    const width = 200;
    const height = Math.max(8, Math.round(width * rect.h / rect.w));

    let best = 0, bestScore = -1;
    for (let deg = -8; deg <= 8; deg += 2) {
        const score = skewScore(
            greyOf(video, rect, width, height, from + deg * Math.PI / 180), width, height);
        if (score > bestScore) { bestScore = score; best = deg; }
    }

    return from + best * Math.PI / 180;
}

/** The zone, levelled and thresholded, with how many pixels each character got. */
function zoneRegion(video, guide, base) {
    const rough = findZone(video, guide, base);
    if (!rough) return null;

    const angle = skewOf(video, rough, base);
    const rect = findZone(video, guide, angle) ?? rough;

    /*  Always rendered at the full width, upscaling a small zone rather than capping at what
     *  the source had. Capping threw away the one thing the recogniser cares about: it wants
     *  characters about thirty pixels tall, and does better with a soft big one than a sharp
     *  small one. What it cannot do is invent detail the camera never captured, which is a
     *  different problem and the one pixelsPerCharacter is reported for. */
    const width = ZONE_WIDTH;
    const height = Math.max(1, Math.round(rect.h * (width / rect.w)));

    const grey = greyOf(video, rect, width, height, angle);
    const binary = localThreshold(grey, width, height);

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;

    const ctx = canvas.getContext('2d');
    const pixels = ctx.createImageData(width, height);

    for (let g = 0; g < binary.length; g++) {
        const o = g * 4;
        pixels.data[o] = pixels.data[o + 1] = pixels.data[o + 2] = binary[g];
        pixels.data[o + 3] = 255;
    }

    ctx.putImageData(pixels, 0, 0);

    // The zone is thirty characters across, so this is what the camera gave each one.
    return { canvas, pixelsPerCharacter: rect.w / 30 };
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
