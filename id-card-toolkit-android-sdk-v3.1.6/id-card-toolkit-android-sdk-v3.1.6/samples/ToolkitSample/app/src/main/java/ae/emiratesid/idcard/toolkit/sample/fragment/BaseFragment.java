package ae.emiratesid.idcard.toolkit.sample.fragment;


import android.app.ProgressDialog;
import androidx.fragment.app.Fragment;
import android.view.View;
import android.view.Window;


public abstract class BaseFragment extends Fragment implements View.OnClickListener {
    private ProgressDialog pd;

    public BaseFragment() {
        // Required empty public constructor
    }

    /**
     * Methods launch Progress dialog to provide user interaction.
     */
    protected void showProgressDialog(String message){
        pd = new ProgressDialog(getActivity());
        pd.requestWindowFeature(Window.FEATURE_NO_TITLE);
        pd.setMessage(message); //recommended to use String resource...
        pd.setCancelable(false);
        pd.show();
    }//

    /**
     * Methods launch Progress dialog to provide user interaction.
     */
    protected void hideProgressDialog(){
        //check is dialog is showing then dismiss it..
        if(pd != null && (pd.isIndeterminate() || pd.isShowing())){
            pd.dismiss();
            pd = null;
        }//
    }//

    @Override
    public void onDetach() {
        super.onDetach();
        hideProgressDialog();
    }

    protected  abstract void appendError(String message);
}
