namespace aTES.Accounting.ApiModels;

public class AccountInfo
{
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public List<TransactionView> Transactions { get; set; }
}