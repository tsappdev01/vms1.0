package ae.dubaiinvestments.vms.ui.theme

import android.app.Activity
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Typography
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalView
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.view.WindowCompat

/*  The palette.

    The same values the web app uses, deliberately - the dark ground is the navy from the
    logo (#0A3255) rather than a neutral grey, because reception sees both this app and
    the web report and two different dark blues would look like a mistake. Light and dark
    are defined in full and separately: a dark theme derived by inverting a light one gets
    the accents wrong, and gold on white is the example. */

private val Navy = Color(0xFF0A3255)
private val NavyMid = Color(0xFF144D7E)
private val Gold = Color(0xFFC8A45C)

/** Gold darkened until it reads on white. The brand gold does not, at text sizes. */
private val GoldOnLight = Color(0xFF8A6A2F)

private val LightScheme = lightColorScheme(
    primary = Navy,
    onPrimary = Color.White,
    primaryContainer = Color(0xFFE6EDF4),
    onPrimaryContainer = Navy,
    secondary = GoldOnLight,
    onSecondary = Color.White,
    secondaryContainer = Color(0xFFF6EEDC),
    onSecondaryContainer = Color(0xFF3D2E10),
    background = Color(0xFFF6F8FA),
    onBackground = Color(0xFF16232E),
    surface = Color.White,
    onSurface = Color(0xFF16232E),
    surfaceVariant = Color(0xFFE9EEF3),
    onSurfaceVariant = Color(0xFF4A5A68),
    outline = Color(0xFFC3CED8),
    outlineVariant = Color(0xFFDCE3E9),
    error = Color(0xFFB3261E),
    onError = Color.White,
    errorContainer = Color(0xFFF9DEDC),
    onErrorContainer = Color(0xFF410E0B),
)

private val DarkScheme = darkColorScheme(
    /* Navy cannot be the primary here - it is the background. Gold is what stays visible
       on it, which is also how the logo uses the two. */
    primary = Gold,
    onPrimary = Color(0xFF2A2008),
    primaryContainer = Color(0xFF1C486E),
    onPrimaryContainer = Color(0xFFE8EEF4),
    secondary = Color(0xFF8FB6D8),
    onSecondary = Color(0xFF072641),
    secondaryContainer = NavyMid,
    onSecondaryContainer = Color(0xFFE8EEF4),
    background = Navy,
    onBackground = Color(0xFFE8EEF4),
    surface = Color(0xFF153E61),
    onSurface = Color(0xFFE8EEF4),
    surfaceVariant = Color(0xFF1C486E),
    onSurfaceVariant = Color(0xFF92A6B6),
    surfaceContainerLowest = Color(0xFF072641),
    outline = Color(0xFF2A557A),
    outlineVariant = Color(0xFF224A6E),
    error = Color(0xFFF2B8B5),
    onError = Color(0xFF601410),
    errorContainer = Color(0xFF33252D),
    onErrorContainer = Color(0xFFF2B8B5),
)

/** Slightly larger than Material's defaults. This is read at arm's length, standing up. */
private val VmsTypography = Typography().run {
    copy(
        headlineSmall = headlineSmall.copy(fontWeight = FontWeight.SemiBold),
        titleMedium = titleMedium.copy(fontWeight = FontWeight.SemiBold),
        bodyLarge = bodyLarge.copy(fontSize = 17.sp, lineHeight = 25.sp),
        bodyMedium = bodyMedium.copy(fontSize = 15.sp, lineHeight = 22.sp),
        labelLarge = labelLarge.copy(fontSize = 15.sp),
    )
}

/**
 * No dynamic colour. Material You would repaint a Dubai Investments application in
 * whatever the tablet's wallpaper happens to be.
 */
@Composable
fun VmsTheme(
    dark: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit,
) {
    val scheme = if (dark) DarkScheme else LightScheme
    val view = LocalView.current

    if (!view.isInEditMode) {
        SideEffect {
            /* The bar above the app bar is navy in both themes, because the app bar is -
               so its icons are light in both. Deriving this from the theme would give
               dark icons on navy in light mode, which is unreadable. */
            (view.context as? Activity)?.window?.let { window ->
                WindowCompat.getInsetsController(window, window.decorView)
                    .isAppearanceLightStatusBars = false
            }
        }
    }

    MaterialTheme(colorScheme = scheme, typography = VmsTypography, content = content)
}

/**
 * The logo navy, for the one place that must not follow the theme: the app bar. It is
 * navy in light mode and navy in dark mode, which is what makes the two look like one
 * application.
 */
val BrandNavy = Navy

/** White, and stated rather than inferred - the app bar's ground is fixed, so its ink is. */
val OnBrandNavy = Color(0xFFF2F6FA)

/** The gap used between cards and around the page. One value, so screens line up. */
val PagePadding = 20.dp
