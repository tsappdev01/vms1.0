package ae.emiratesid.idcard.toolkit.sample.widget;

import android.annotation.TargetApi;
import android.content.Context;
import android.graphics.Color;
import android.os.Build;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.style.ForegroundColorSpan;
import android.util.AttributeSet;
import android.widget.TextView;

import ae.emiratesid.idcard.toolkit.sample.logger.Logger;

public class LogTextView extends TextView {

    public static class LOG_TYPE{
        public static final int ERROR =1;
        public static final int SUCCESS=2;
        public static final int INFO=3;
    };

    private String data;
    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public LogTextView(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
    }

    public LogTextView(Context context) {
        super(context);
    }

    public LogTextView(Context context, AttributeSet attrs) {
        super(context, attrs);
    }

    public LogTextView(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
    }

    public void setLog(String log , int type) {
        //check if null
        if( log == null){
            return;
        }//
        setText(log);
        int color =  Color.GRAY;
        switch(type){
            case LOG_TYPE.ERROR:
                color =  Color.parseColor("#e74c3c");
                break;
            case LOG_TYPE.SUCCESS:
                color =  Color.parseColor("#16a085");
                break;
            case LOG_TYPE.INFO:
                color = Color.parseColor("#34495e");
                break;
            default:
                color = Color.GRAY;
        }//type
        setTextColor(color);
    }//setData()
    public void appendLog(String log , int type){
        if( log == null || log.isEmpty()){
            return;
        }//
        SpannableString sb =  new SpannableString(log);
        int logLength =log.length();
        int color = Color.GRAY;
        Logger.d("type" +type);
        switch(type){
            case LOG_TYPE.ERROR:

                color =  Color.parseColor("#e74c3c");
                break;
            case LOG_TYPE.SUCCESS:
                color =  Color.parseColor("#16a085");
                break;
            case LOG_TYPE.INFO:
                color = Color.parseColor("#34495e"); //blue
                break;
            default:
               color = Color.GRAY;
        }//type
        //set the span to Spannable
        Logger.d("Color::"+color + " LogLength " + logLength);
        sb.setSpan(new ForegroundColorSpan(color), 0, logLength, Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
        this.append(sb);
    }//appendLog()
}
