-- Sample row matching NBA_AzQR+PosterminalAPI+Functions(V6).pdf (CAFE CITY BAKU / V1E0230675).
IF NOT EXISTS (SELECT 1 FROM dbo.Terminals WHERE SerialNumber = N'V1E0230675')
BEGIN
    INSERT INTO dbo.Terminals
    (
        SerialNumber, TerminalNo, TerminalModel, TerminalType,
        CountryCode, HeaderCountryCode, CurrencyCode, CurrencyNumericCode,
        TerminalLanguageCode, ReceiptLanguageCode, TimeZone, RrnPrefix,
        CompanyId, CompanyCode, CompanyName, MerchantId, RegisterId, RegisterTsmId,
        MerchantName, MerchantAddress1, PhoneNumber, CategoryCode, MerchantNo, Email,
        TaxNumber, City, PostalCode, BranchName,
        AliasType, AliasValue, BankBic, ProviderBic, OperationCode, TransactionType,
        IpsSpecVersion, IpsUuid, DeliveryChannel, Coordinates
    )
    VALUES
    (
        N'V1E0230675', N'NBA12345', N'X990', '02',
        'AZ', N'AZE', 'AZN', '944',
        N'az', N'az', N'Asia/Baku', N'NBA',
        3, N'NBA', N'NBA', 5, 32, '29526D61-933F-440F-A2EF-ADEF8BDD4F39',
        N'CAFE CITY BAKU', N'Nizami str. 65', N'+994503595558', '5812', N'11205228', N'',
        N'1001812792', N'BAKU', N'AZ1014', N'CAFE CITY BAKU',
        '04', N'AZ49IBAZ40050019449333061204', N'IBAZAZ20', N'NBATAZ20', N'MPRQ-ATP', '613',
        'MPV002', '8779c7cfceb149b89546c4f3faea3721', '400', NULL
    );
END
GO
