package ae.emiratesid.idcard.toolkit.sample.utils;

import android.os.Environment;
import android.provider.Settings;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.io.File;
import java.text.DateFormat;
import java.util.Calendar;
import java.util.GregorianCalendar;

import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class LogUtils {

    public static void appendLog(String text)
    {
        String file = Environment.getExternalStorageDirectory().getAbsolutePath()+"/EIDAToolkit/Applog.txt";

        File logFile = new File(file);
        if (!logFile.exists())
        {
            try
            {
                logFile.createNewFile();
            }
            catch (IOException e)
            {
                // TODO Auto-generated catch block
                e.printStackTrace();
            }
        }
        try
        {
            //BufferedWriter for performance, true to set append to file flag
            BufferedWriter buf = new BufferedWriter(new FileWriter(logFile, true));
            DateFormat df = DateFormat.getDateTimeInstance();
            Calendar cal = new GregorianCalendar();
            buf.append(df.format(cal.getTime()) +": " +text);
            buf.newLine();
            buf.close();
        }
        catch (IOException e)
        {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }
    }
}
