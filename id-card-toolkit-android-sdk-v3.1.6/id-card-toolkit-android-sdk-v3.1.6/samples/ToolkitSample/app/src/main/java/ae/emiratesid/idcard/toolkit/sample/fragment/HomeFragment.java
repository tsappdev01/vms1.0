package ae.emiratesid.idcard.toolkit.sample.fragment;


import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;

import ae.emiratesid.idcard.toolkit.sample.R;

public class HomeFragment extends BaseFragment {


    public HomeFragment() {
        // Required empty public constructor
    }

    @Override
    protected void appendError(String message) {

    }

    Button btnConnect , btnDisconnect;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
      View view =   inflater.inflate(R.layout.fragment_home, container, false);
        return view;
    }//

    @Override
    public void onClick(View view) {

    }
}
