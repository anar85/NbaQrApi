-- NBA AzQR terminal store. All QR and POS API fields are read from this table.
IF OBJECT_ID(N'dbo.QrPayments', N'U') IS NOT NULL DROP TABLE dbo.QrPayments;
IF OBJECT_ID(N'dbo.Terminals', N'U') IS NOT NULL DROP TABLE dbo.Terminals;
GO

CREATE TABLE dbo.Terminals
(
    Id                      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Terminals PRIMARY KEY,
    SerialNumber            NVARCHAR(50)  NOT NULL,
    TerminalNo              NVARCHAR(25)  NOT NULL,
    TerminalModel           NVARCHAR(50)  NULL,
    TerminalType            CHAR(2)       NOT NULL CONSTRAINT DF_Terminals_TerminalType DEFAULT ('02'), -- IPS 26-04
    CountryCode             CHAR(2)       NOT NULL CONSTRAINT DF_Terminals_CountryCode DEFAULT ('AZ'), -- IPS 58 ISO 3166-1 alpha-2
    HeaderCountryCode       NVARCHAR(3)   NOT NULL CONSTRAINT DF_Terminals_HeaderCountry DEFAULT ('AZE'),
    CurrencyCode            CHAR(3)       NOT NULL CONSTRAINT DF_Terminals_Currency DEFAULT ('AZN'),
    CurrencyNumericCode     CHAR(3)       NOT NULL CONSTRAINT DF_Terminals_CurrencyNum DEFAULT ('944'), -- IPS 53 ISO 4217
    TerminalLanguageCode    NVARCHAR(5)   NOT NULL CONSTRAINT DF_Terminals_TermLang DEFAULT ('az'),
    ReceiptLanguageCode     NVARCHAR(5)   NOT NULL CONSTRAINT DF_Terminals_RcptLang DEFAULT ('az'),
    TimeZone                NVARCHAR(50)  NOT NULL CONSTRAINT DF_Terminals_Tz DEFAULT ('Asia/Baku'),
    RrnPrefix               NVARCHAR(10)  NOT NULL,

    CompanyId               INT           NULL,
    CompanyCode             NVARCHAR(20)  NOT NULL,
    CompanyName             NVARCHAR(100) NOT NULL,
    MerchantId              INT           NULL,
    RegisterId              INT           NULL,
    RegisterTsmId           UNIQUEIDENTIFIER NULL,

    MerchantName            NVARCHAR(25)  NOT NULL, -- IPS 59
    MerchantAddress1        NVARCHAR(200) NULL,
    PhoneNumber             NVARCHAR(30)  NULL,
    CategoryCode            CHAR(4)       NOT NULL, -- IPS 52 MCC
    MerchantNo              NVARCHAR(35)  NOT NULL, -- IPS 27-03 object identifier
    Email                   NVARCHAR(100) NULL,
    TaxNumber               NVARCHAR(10)  NULL,     -- IPS 62-10 TIN
    City                    NVARCHAR(15)  NOT NULL, -- IPS 60
    PostalCode              NVARCHAR(10)  NULL,     -- IPS 61
    BranchName              NVARCHAR(25)  NOT NULL, -- IPS 62-03 mandatory when 62 present

    AliasType               CHAR(2)       NOT NULL CONSTRAINT DF_Terminals_AliasType DEFAULT ('04'), -- IPS 27-00
    AliasValue              NVARCHAR(35)  NOT NULL, -- IPS 27-01 IBAN/TIN/mobile/...
    BankBic                 NVARCHAR(11)  NULL,     -- IPS 27-02 required when alias is IBAN
    ProviderBic             NVARCHAR(11)  NOT NULL, -- IPS 36-00
    OperationCode           NVARCHAR(10)  NOT NULL CONSTRAINT DF_Terminals_OpCode DEFAULT ('MPRQ-ATP'), -- IPS 36-01
    TransactionType         CHAR(3)       NOT NULL CONSTRAINT DF_Terminals_TxnType DEFAULT ('613'), -- IPS 36-02
    IpsSpecVersion          CHAR(6)       NOT NULL CONSTRAINT DF_Terminals_IpsVer DEFAULT ('MPV002'), -- IPS 39
    IpsUuid                 CHAR(32)      NOT NULL, -- IPS 40, hyphens stripped
    DeliveryChannel         CHAR(3)       NOT NULL CONSTRAINT DF_Terminals_Delivery DEFAULT ('400'), -- IPS 62-11
    Coordinates             CHAR(16)      NULL,     -- IPS 28 latitude+longitude digits
    ConsumerInfoQuery       NVARCHAR(3)   NULL,     -- IPS 62-09 A/B/E
    TipFeeType              CHAR(2)       NULL,     -- IPS 55
    FixedConvenienceFee     NVARCHAR(13)  NULL,     -- IPS 56
    ConvenienceFeePercent   NVARCHAR(5)   NULL,     -- IPS 57
    AltLanguageCode         CHAR(2)       NULL,     -- IPS 64-00
    AltMerchantName         NVARCHAR(25)  NULL,     -- IPS 64-01
    AltCity                 NVARCHAR(15)  NULL,     -- IPS 64-02

    IsActive                BIT           NOT NULL CONSTRAINT DF_Terminals_IsActive DEFAULT (1),
    CreatedAtUtc            DATETIME2(3)  NOT NULL CONSTRAINT DF_Terminals_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc            DATETIME2(3)  NOT NULL CONSTRAINT DF_Terminals_Updated DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT UQ_Terminals_SerialNumber UNIQUE (SerialNumber)
);
GO

CREATE TABLE dbo.QrPayments
(
    Id                      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_QrPayments PRIMARY KEY,
    UniqueId                NVARCHAR(32)  NOT NULL,
    EndToEndId              NVARCHAR(32)  NOT NULL,
    SerialNumber            NVARCHAR(50)  NOT NULL,
    PaymentType             INT           NOT NULL,
    TotalAmount             DECIMAL(13, 2) NOT NULL,
    QrCodeStr               NVARCHAR(MAX) NOT NULL,
    StatusCode              INT           NOT NULL,
    StatusDesc              NVARCHAR(200) NOT NULL,
    MerchantNo              NVARCHAR(35)  NOT NULL,
    TerminalNo              NVARCHAR(25)  NOT NULL,
    CurrencyCode            CHAR(3)       NOT NULL,
    RefundedPaymentId       BIGINT        NULL,
    RefundedUniqueId        NVARCHAR(32)  NULL,
    RefundedEndToEndId      NVARCHAR(32)  NULL,
    IpsStatus               NVARCHAR(50)  NULL,
    IsCanceled              BIT           NOT NULL CONSTRAINT DF_QrPayments_IsCanceled DEFAULT (0),
    Description             NVARCHAR(200) NULL,
    CreatedAtUtc            DATETIME2(3)  NOT NULL CONSTRAINT DF_QrPayments_Created DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc            DATETIME2(3)  NOT NULL CONSTRAINT DF_QrPayments_Updated DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT UQ_QrPayments_UniqueId UNIQUE (UniqueId),
    CONSTRAINT FK_QrPayments_Terminals FOREIGN KEY (SerialNumber) REFERENCES dbo.Terminals (SerialNumber)
);
GO

CREATE INDEX IX_QrPayments_SerialNumber ON dbo.QrPayments (SerialNumber, CreatedAtUtc DESC);
GO
