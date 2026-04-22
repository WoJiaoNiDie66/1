using Firebase.Firestore;

[FirestoreData]
public class BridgeData
{
    [FirestoreProperty] public string Id { get; set; } // 桥梁的唯一标识符
    [FirestoreProperty] public string DestinationName { get; set; }
    [FirestoreProperty] public int CurrentQuantity { get; set; }  // 符文石上的留言
    [FirestoreProperty] public int RequiredQuantity { get; set; } // 所属区块 ID（用于快速查询）

}
