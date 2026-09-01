package ae.emiratesid.idcard.toolkit.sample.utils;

import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.util.Log;

public class Bitmaps {


    public static Bitmap decodeSampledBitmapFromResource(Resources res, int resId,
                                                         int reqWidth, int reqHeight) {

        // First decode with inJustDecodeBounds=true to check dimensions
        final BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        BitmapFactory.decodeResource(res, resId, options);

        // Calculate inSampleSize
        options.inSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);

        // Decode bitmap with inSampleSize set
        options.inJustDecodeBounds = false;
        return BitmapFactory.decodeResource(res, resId, options);
    }//decodeSampledBitmapFromResource()...

    public static Bitmap decodeSampledBitmapFromBytes(byte[] imgData,
                                                      int reqWidth, int reqHeight) {

        // First decode with inJustDecodeBounds=true to check dimensions
        final BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        BitmapFactory.decodeByteArray(imgData, 0, imgData.length, options);
        Log.d("Bitmaps", "decodeSampledBitmapFromBytes()::" + imgData.length);

        // Calculate inSampleSize
        options.inSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);

        // Decode bitmap with inSampleSize set
        options.inJustDecodeBounds = false;
        Bitmap bit = BitmapFactory.decodeByteArray(imgData, 0, imgData.length, options);
        //Log.d("Bitmaps", "decodeSampledBitmapFromBytes()::"+bit.getHeight()+"::"+bit.getDensity());
        //Log.d("Bitmaps", "Height :width::" + bit.getHeight() +":"+bit.getWidth());
        return bit;
    }//decodeSampledBitmapFromResource()...

    public static Bitmap decodeSampledBitmapFromFile(String path,
                                                     int reqWidth, int reqHeight) {

        // First decode with inJustDecodeBounds=true to check dimensions
        final BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        BitmapFactory.decodeFile(path, options);

        // Calculate inSampleSize
        options.inSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);

        // Decode bitmap with inSampleSize set
        options.inJustDecodeBounds = false;
        Bitmap bit = BitmapFactory.decodeFile(path, options);
        //Log.d("Bitmaps", "decodeSampledBitmapFromBytes()::"+bit.getHeight()+"::"+bit.getDensity());
        return bit;
    }//decodeSampledBitmapFromResource()...

    private static int calculateInSampleSize(
            BitmapFactory.Options options, int reqWidth, int reqHeight) {
        // Raw height and width of image
        final int height = options.outHeight;
        final int width = options.outWidth;
        int inSampleSize = 1;

        if (height > reqHeight || width > reqWidth) {

            final int halfHeight = height / 2;
            final int halfWidth = width / 2;

            // Calculate the largest inSampleSize value that is a power of 2 and keeps both
            // height and width larger than the requested height and width.
            while ((halfHeight / inSampleSize) > reqHeight
                    && (halfWidth / inSampleSize) > reqWidth) {
                inSampleSize *= 2;
            }//while()....
        }
        Log.d("Bitmaps", "calculateInSampleSize()::" + inSampleSize);
        return inSampleSize;
    }//calculateInSampleSize()...

    public static Bitmap rotateBitmap(Bitmap source, float angle)

    {
        Matrix matrix = new Matrix();
        matrix.postRotate(angle);

        return Bitmap.createBitmap(source, 0, 0, source.getWidth(),
                source.getHeight(), matrix, true);
    }//rotateBitmap()...

}//Bitmaps
