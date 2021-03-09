using ControlFlowPractise.ComprehensiveData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Core.Tests.WarrantyServiceTestData
{
    public class TestData
    {
        public string? TargetTest { get; set; }
        public List<WarrantyCaseVerificationTestData> WarrantyCaseVerificationTestDatas { get; set; } =
            new List<WarrantyCaseVerificationTestData>();
    }

    public class WarrantyCaseVerificationTestData
    {
        public int InsertOrder { get; set; }
        public WarrantyCaseVerification WarrantyCaseVerification { get; set; }
        public WarrantyCaseVerificationTestData(WarrantyCaseVerification warrantyCaseVerification)
        {
            WarrantyCaseVerification = warrantyCaseVerification;
        }
    }
}
