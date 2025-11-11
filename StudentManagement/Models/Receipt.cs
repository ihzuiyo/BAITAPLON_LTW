using System;
using System.Collections.Generic;

namespace StudentManagement.Models;

public partial class Receipt
{
    public int ReceiptId { get; set; }

    public int TuitionId { get; set; }

    public int CashierId { get; set; }

    public string ReceiptCode { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Note { get; set; }

    public virtual User Cashier { get; set; } = null!;

    public virtual Tuition Tuition { get; set; } = null!;
}
