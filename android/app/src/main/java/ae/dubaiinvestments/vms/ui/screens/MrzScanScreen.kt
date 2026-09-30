package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.card.MrzAnalyzer
import ae.dubaiinvestments.vms.ui.UiState
import android.Manifest
import android.content.pm.PackageManager
import android.util.Size
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.camera.core.CameraSelector
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.Preview
import androidx.camera.core.resolutionselector.ResolutionSelector
import androidx.camera.core.resolutionselector.ResolutionStrategy
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.foundation.border
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalLifecycleOwner
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.content.ContextCompat
import ae.dubaiinvestments.vms.ui.parts.SectionCard

/**
 * Reading the card with the camera, for a visitor who carries it on a phone.
 *
 * Two things decide whether this works, and neither is the recogniser.
 *
 * **Resolution.** The zone is thirty characters across roughly a third of the frame's width
 * when a card is held at a comfortable distance. CameraX defaults image analysis to 640x480,
 * which puts a character at about eight pixels - unreadable by anything, and no amount of work
 * downstream recovers detail the camera never captured. The browser scanner spent several
 * rounds learning that; here it is one [ResolutionSelector].
 *
 * **Not calling the server on every frame.** Every rule about whether text is a card lives on
 * the server, but a round trip per frame would make the loop as slow as the network. So each
 * frame is filtered here first, on something no recogniser can be wrong about: the zone is
 * padded with chevrons and the printed number starts 784. Text with neither is not a card, and
 * is dropped without a call. In practice nothing is sent until the card is actually in view.
 */
@Composable
fun MrzScanScreen(
    state: UiState,
    onText: (String) -> Unit,
    onClose: () -> Unit,
) {
    val context = LocalContext.current

    var granted by remember {
        mutableStateOf(
            ContextCompat.checkSelfPermission(context, Manifest.permission.CAMERA)
                == PackageManager.PERMISSION_GRANTED,
        )
    }

    val ask = rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()) {
        granted = it
    }

    LaunchedEffect(Unit) {
        if (!granted) ask.launch(Manifest.permission.CAMERA)
    }

    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
        SectionCard {
            Text("Scan the card", style = MaterialTheme.typography.headlineSmall)
            Text(
                "Hold the card inside the frame, filling it. Either side, either way up. " +
                    "It is read while you hold it and there is nothing to press.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )

            if (!granted) {
                Text(
                    "This tablet has not allowed the camera. Allow it to scan a card; the " +
                        "reader on the desk is unaffected either way.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.error,
                )
                Button(onClick = { ask.launch(Manifest.permission.CAMERA) }) {
                    Text("Allow the camera")
                }
            } else {
                Box(
                    Modifier
                        .fillMaxWidth()
                        .aspectRatio(4f / 3f)
                        .clip(RoundedCornerShape(12.dp)),
                    contentAlignment = Alignment.Center,
                ) {
                    CameraPreview(onText = onText)

                    /* The card is ID-1, so the guide is ID-1. A box of any other shape invites
                       the officer to fill it with a card that is then either cropped or too
                       far away, and too far away is the failure that looks like a broken
                       scanner. */
                    Box(
                        Modifier
                            .fillMaxWidth(0.86f)
                            .aspectRatio(85.6f / 54f)
                            .border(2.dp, Color.White.copy(alpha = 0.9f), RoundedCornerShape(8.dp)),
                    )
                }

                Row(verticalAlignment = Alignment.CenterVertically) {
                    if (state.busy != null) {
                        CircularProgressIndicator(Modifier.size(18.dp), strokeWidth = 2.dp)
                    }
                    Text(
                        state.busy ?: "Reading…",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                        modifier = Modifier.padding(start = 10.dp),
                    )
                }
            }

            TextButton(onClick = onClose) { Text("Cancel") }
        }
    }
}

@Composable
private fun CameraPreview(onText: (String) -> Unit) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current
    val analyzer = remember { MrzAnalyzer(onText) }

    DisposableEffect(Unit) {
        onDispose { analyzer.close() }
    }

    AndroidView(
        factory = { ctx ->
            val view = PreviewView(ctx).apply {
                scaleType = PreviewView.ScaleType.FILL_CENTER
            }

            val providerFuture = ProcessCameraProvider.getInstance(ctx)
            providerFuture.addListener({
                val provider = providerFuture.get()

                val preview = Preview.Builder().build().also {
                    it.surfaceProvider = view.surfaceProvider
                }

                /*  1920x1080 for analysis, not the 640x480 CameraX would choose.
                 *
                 *  This is the whole difference between a scanner that reads and one that
                 *  returns the ID number and nothing else. CLOSEST_HIGHER_THEN_LOWER so a
                 *  tablet that cannot manage it gives its best rather than refusing. */
                val analysis = ImageAnalysis.Builder()
                    .setResolutionSelector(
                        ResolutionSelector.Builder()
                            .setResolutionStrategy(
                                ResolutionStrategy(
                                    Size(1920, 1080),
                                    ResolutionStrategy.FALLBACK_RULE_CLOSEST_HIGHER_THEN_LOWER,
                                ),
                            )
                            .build(),
                    )
                    /* The newest frame, never a queue. A backlog means answering for a
                       picture the card has already left. */
                    .setBackpressureStrategy(ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST)
                    .build()
                    .also { it.setAnalyzer(ContextCompat.getMainExecutor(ctx), analyzer) }

                runCatching {
                    provider.unbindAll()
                    provider.bindToLifecycle(
                        lifecycleOwner,
                        CameraSelector.DEFAULT_BACK_CAMERA,
                        preview,
                        analysis,
                    )
                }
            }, ContextCompat.getMainExecutor(ctx))

            view
        },
        modifier = Modifier.fillMaxWidth(),
    )
}
