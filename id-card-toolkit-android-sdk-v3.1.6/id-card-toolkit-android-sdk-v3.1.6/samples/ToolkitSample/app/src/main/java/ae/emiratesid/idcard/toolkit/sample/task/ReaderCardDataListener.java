package ae.emiratesid.idcard.toolkit.sample.task;

import ae.emiratesid.idcard.toolkit.datamodel.CardPublicData;

public interface ReaderCardDataListener {
    public void onCardReadComplete(int status, String message , CardPublicData cardPublicData);
}
