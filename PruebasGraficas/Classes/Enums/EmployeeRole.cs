using System.ComponentModel;

namespace PruebasGraficas.Classes.Enums
{
    [Flags]
    public enum EmployeeRole
    {
        [Description("Dispatcher")]
        Dispatcher = 1,
        [Description("Manager")]
        Manager = 2,
        [Description("Sub-Administrator")]
        SubAdministrator = 4,
        [Description("Sales (Level 1)")]
        SalesLevel1 = 8,
        [Description("Sales (Level 2)")]
        SalesLevel2 = 16,
        [Description("Sales (Level 3)")]
        SalesLevel3 = 32,
        [Description("Customer Support (Level 1)")]
        CustomerSupportLevel1 = 64,
        [Description("Customer Support (Level 2)")]
        CustomerSupportLevel2 = 128,
        [Description("Accounting")]
        Accounting = 256,
        [Description("Business Analyst")]
        BusinessAnalyst = 512,
    }
}
