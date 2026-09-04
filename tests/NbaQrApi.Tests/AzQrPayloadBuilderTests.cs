using NbaQrApi.AzQr;

namespace NbaQrApi.Tests;

public class AzQrPayloadBuilderTests
{
    private const string IpsUuid = "8779c7cfceb149b89546c4f3faea3721";

    [Fact]
    public void Build_matches_spec_example_1_retail_dynamic_pos()
    {
        var payload = new AzQrPayload
        {
            IpsSpecVersion = "MPV002",
            IpsUuid = IpsUuid,
            QrCodeType = AzQrPayloadBuilder.DynamicQr,
            Main = new AzQrMainData
            {
                UniqueIdentifier = "12346578634567",
                TerminalType = "02"
            },
            Presenter = new AzQrPresenter
            {
                AliasType = "05",
                AliasValue = "9345678945"
            },
            Coordinates = "3993942332851791",
            Ips = new AzQrIpsSpecific
            {
                ProviderBic = "PORTAL20MEP",
                OperationCode = "MPRQ-ATP",
                TransactionType = "613"
            },
            Additional = new AzQrAdditionalInfo
            {
                TerminalNumber = "7643565767"
            },
            MerchantCategoryCode = "5812",
            CurrencyNumericCode = "944",
            Amount = "56.90",
            TipFeeType = "03",
            ConvenienceFeePercent = "05",
            CountryCode = "AZ",
            MerchantName = "Portofino",
            City = "Baku",
            PostalCode = "AZ1014",
            AdditionalData = new AzQrAdditionalData
            {
                BranchName = "Portofino Gənclik",
                ConsumerInfoQuery = "B",
                TaxNumber = "9345678945",
                DeliveryChannel = "400"
            }
        };

        var qr = AzQrPayloadBuilder.Build(payload);

        const string expected =
            "3906MPV00240328779c7cfceb149b89546c4f3faea372100020101021226300002010314123465786345670402022720000205011093456789452816399394233285179136340011PORTAL20MEP0108MPRQ-ATP0203613371407107643565767520458125303944540556.905502035702055802AZ5909Portofino6004Baku6106AZ101462470317Portofino Gənclik0901B101093456789451103400630403A4";

        Assert.Equal(expected[..^4], qr[..^4]);
        Assert.Equal(4, qr[^4..].Length);
        Assert.Matches("^[0-9A-F]{4}$", qr[^4..]);
    }

    [Fact]
    public void Build_matches_nba_terminal_api_sample_payload()
    {
        var payload = new AzQrPayload
        {
            IpsSpecVersion = "MPV002",
            IpsUuid = IpsUuid,
            QrCodeType = AzQrPayloadBuilder.DynamicQr,
            Main = new AzQrMainData
            {
                UniqueIdentifier = "NBA0W4C2HPZC",
                TerminalType = "02"
            },
            Presenter = new AzQrPresenter
            {
                AliasType = "04",
                AliasValue = "AZ49IBAZ40050019449333061204",
                BankBic = "IBAZAZ20",
                ObjectIdentifier = "11205228"
            },
            Ips = new AzQrIpsSpecific
            {
                ProviderBic = "NBATAZ20",
                OperationCode = "MPRQ-ATP",
                TransactionType = "613"
            },
            Additional = new AzQrAdditionalInfo
            {
                TerminalNumber = "V1E0230675"
            },
            MerchantCategoryCode = "5812",
            CurrencyNumericCode = "944",
            Amount = "01.23",
            CountryCode = "AZ",
            MerchantName = "CAFE CITY BAKU",
            City = "BAKU",
            PostalCode = "AZ1014",
            AdditionalData = new AzQrAdditionalData
            {
                BranchName = "CAFE CITY BAKU",
                TaxNumber = "1001812792",
                DeliveryChannel = "400"
            }
        };

        var qr = AzQrPayloadBuilder.Build(payload);

        const string expected =
            "3906MPV00240328779c7cfceb149b89546c4f3faea372100020101021226280002010312NBA0W4C2HPZC04020227620002040128AZ49IBAZ400500194493330612040208IBAZAZ2003081120522836310008NBATAZ200108MPRQ-ATP020361337140710V1E0230675520458125303944540501.235802AZ5914CAFE CITY BAKU6004BAKU6106AZ101462390314CAFE CITY BAKU10101001812792110340063041CC9";

        Assert.Equal(expected, qr);
    }

    [Fact]
    public void Build_adds_return_attribute_for_refund()
    {
        var payload = new AzQrPayload
        {
            IpsSpecVersion = "MPV002",
            IpsUuid = IpsUuid,
            QrCodeType = AzQrPayloadBuilder.DynamicQr,
            Main = new AzQrMainData
            {
                UniqueIdentifier = "12346578634567",
                TerminalType = "02"
            },
            Presenter = new AzQrPresenter
            {
                AliasType = "05",
                AliasValue = "9345678945"
            },
            Coordinates = "3993942332851791",
            Ips = new AzQrIpsSpecific
            {
                ProviderBic = "PORTAL20MEP",
                OperationCode = "MPRQ-ATP",
                TransactionType = "613"
            },
            Additional = new AzQrAdditionalInfo
            {
                TerminalNumber = "7643565767",
                ReturnAttribute = "RT"
            },
            MerchantCategoryCode = "5812",
            CurrencyNumericCode = "944",
            Amount = "56.90",
            TipFeeType = "03",
            ConvenienceFeePercent = "05",
            CountryCode = "AZ",
            MerchantName = "Portofino",
            City = "Baku",
            PostalCode = "AZ1014",
            AdditionalData = new AzQrAdditionalData
            {
                BranchName = "Portofino Gənclik",
                ConsumerInfoQuery = "B",
                TaxNumber = "9345678945",
                DeliveryChannel = "400"
            }
        };

        var qr = AzQrPayloadBuilder.Build(payload);

        Assert.Contains("1102RT", qr);
        Assert.Contains("6304", qr);
        Assert.Matches("^[0-9A-F]{4}$", qr[^4..]);
    }

    [Fact]
    public void Build_rejects_dynamic_qr_without_unique_id()
    {
        var payload = new AzQrPayload
        {
            IpsSpecVersion = "MPV002",
            IpsUuid = IpsUuid,
            QrCodeType = AzQrPayloadBuilder.DynamicQr,
            Main = new AzQrMainData { TerminalType = "01" },
            Presenter = new AzQrPresenter { AliasType = "05", AliasValue = "9345678945" },
            Ips = new AzQrIpsSpecific { OperationCode = "MPRQ", TransactionType = "613" },
            MerchantCategoryCode = "5812",
            CurrencyNumericCode = "944",
            CountryCode = "AZ",
            MerchantName = "Test",
            City = "Baku"
        };

        Assert.Throws<AzQrValidationException>(() => AzQrPayloadBuilder.Build(payload));
    }
}
