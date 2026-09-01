package ae.emiratesid.idcard.toolkit.sample;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

public class GsonUtils {

	private static GsonBuilder builder = null;
    private static Gson gson = null;

    static {
        builder = new GsonBuilder();
        gson = builder.create();
    }
    
    public static <T> T parseJson(String jsonString, Class<T> cls) {
        if (jsonString == null || jsonString.isEmpty()) {
            throw new IllegalArgumentException("JSON parameter string must not be null or empty");
        }
        return gson.fromJson(jsonString, cls);
    }
}
