using Firebase.Firestore;

[FirestoreData]
public class BridgeData
{
    [FirestoreProperty] public int bridgeID { get; set; }
    [FirestoreProperty] public string CurrentLocationName { get; set; }
    [FirestoreProperty] public string DestinationName { get; set; }
    [FirestoreProperty] public int CoinsRequired { get; set; }  
    [FirestoreProperty] public int CoinsStored { get; set; }

    public override string ToString()
    {
        string result = $"Bridge ID: {bridgeID}\n" +
                        $"From: {CurrentLocationName}\n" +
                        $"To: {DestinationName}\n" +
                        $"Stored Coins: {CoinsStored}\n"+
                        $"Required Coins: {CoinsRequired}\n";
        return result;
    }

}
