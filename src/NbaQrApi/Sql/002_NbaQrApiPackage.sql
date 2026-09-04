CREATE OR REPLACE PACKAGE nba_qr_api_pkg AS
    PROCEDURE get_terminal_by_serial(
        p_serial_number IN terminals.serial_number%TYPE,
        p_result OUT SYS_REFCURSOR);

    PROCEDURE unique_id_exists(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_exists OUT NUMBER);

    PROCEDURE insert_qr_payment(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_end_to_end_id IN qr_payments.end_to_end_id%TYPE,
        p_serial_number IN qr_payments.serial_number%TYPE,
        p_payment_type IN qr_payments.payment_type%TYPE,
        p_total_amount IN qr_payments.total_amount%TYPE,
        p_qr_code_str IN qr_payments.qr_code_str%TYPE,
        p_status_code IN qr_payments.status_code%TYPE,
        p_status_desc IN qr_payments.status_desc%TYPE,
        p_merchant_no IN qr_payments.merchant_no%TYPE,
        p_terminal_no IN qr_payments.terminal_no%TYPE,
        p_currency_code IN qr_payments.currency_code%TYPE,
        p_refunded_payment_id IN qr_payments.refunded_payment_id%TYPE,
        p_refunded_unique_id IN qr_payments.refunded_unique_id%TYPE,
        p_refunded_end_to_end_id IN qr_payments.refunded_end_to_end_id%TYPE,
        p_ips_status IN qr_payments.ips_status%TYPE,
        p_is_canceled IN qr_payments.is_canceled%TYPE,
        p_description IN qr_payments.description%TYPE,
        p_id OUT qr_payments.id%TYPE);

    PROCEDURE get_qr_payment_by_unique_id(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_result OUT SYS_REFCURSOR);

    PROCEDURE update_qr_payment_status(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_status_code IN qr_payments.status_code%TYPE,
        p_status_desc IN qr_payments.status_desc%TYPE,
        p_is_canceled IN qr_payments.is_canceled%TYPE,
        p_ips_status IN qr_payments.ips_status%TYPE,
        p_description IN qr_payments.description%TYPE);

    PROCEDURE seed_sample_terminal;
END nba_qr_api_pkg;
/

CREATE OR REPLACE PACKAGE BODY nba_qr_api_pkg AS
    PROCEDURE get_terminal_by_serial(
        p_serial_number IN terminals.serial_number%TYPE,
        p_result OUT SYS_REFCURSOR) AS
    BEGIN
        OPEN p_result FOR
            SELECT
                id AS "Id",
                serial_number AS "SerialNumber",
                terminal_no AS "TerminalNo",
                terminal_model AS "TerminalModel",
                terminal_type AS "TerminalType",
                country_code AS "CountryCode",
                header_country_code AS "HeaderCountryCode",
                currency_code AS "CurrencyCode",
                currency_numeric_code AS "CurrencyNumericCode",
                terminal_language_code AS "TerminalLanguageCode",
                receipt_language_code AS "ReceiptLanguageCode",
                time_zone AS "TimeZone",
                rrn_prefix AS "RrnPrefix",
                company_id AS "CompanyId",
                company_code AS "CompanyCode",
                company_name AS "CompanyName",
                merchant_id AS "MerchantId",
                register_id AS "RegisterId",
                RAWTOHEX(register_tsm_id) AS "RegisterTsmId",
                merchant_name AS "MerchantName",
                merchant_address1 AS "MerchantAddress1",
                phone_number AS "PhoneNumber",
                category_code AS "CategoryCode",
                merchant_no AS "MerchantNo",
                email AS "Email",
                tax_number AS "TaxNumber",
                city AS "City",
                postal_code AS "PostalCode",
                branch_name AS "BranchName",
                alias_type AS "AliasType",
                alias_value AS "AliasValue",
                bank_bic AS "BankBic",
                provider_bic AS "ProviderBic",
                operation_code AS "OperationCode",
                transaction_type AS "TransactionType",
                ips_spec_version AS "IpsSpecVersion",
                ips_uuid AS "IpsUuid",
                delivery_channel AS "DeliveryChannel",
                coordinates AS "Coordinates",
                consumer_info_query AS "ConsumerInfoQuery",
                tip_fee_type AS "TipFeeType",
                fixed_convenience_fee AS "FixedConvenienceFee",
                convenience_fee_percent AS "ConvenienceFeePercent",
                alt_language_code AS "AltLanguageCode",
                alt_merchant_name AS "AltMerchantName",
                alt_city AS "AltCity",
                is_active AS "IsActive"
            FROM terminals
            WHERE serial_number = p_serial_number
              AND is_active = 1
              AND ROWNUM = 1;
    END get_terminal_by_serial;

    PROCEDURE unique_id_exists(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_exists OUT NUMBER) AS
        v_count NUMBER(1);
    BEGIN
        SELECT COUNT(1)
        INTO v_count
        FROM qr_payments
        WHERE unique_id = p_unique_id
          AND ROWNUM = 1;

        p_exists := CASE WHEN v_count > 0 THEN 1 ELSE 0 END;
    END unique_id_exists;

    PROCEDURE insert_qr_payment(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_end_to_end_id IN qr_payments.end_to_end_id%TYPE,
        p_serial_number IN qr_payments.serial_number%TYPE,
        p_payment_type IN qr_payments.payment_type%TYPE,
        p_total_amount IN qr_payments.total_amount%TYPE,
        p_qr_code_str IN qr_payments.qr_code_str%TYPE,
        p_status_code IN qr_payments.status_code%TYPE,
        p_status_desc IN qr_payments.status_desc%TYPE,
        p_merchant_no IN qr_payments.merchant_no%TYPE,
        p_terminal_no IN qr_payments.terminal_no%TYPE,
        p_currency_code IN qr_payments.currency_code%TYPE,
        p_refunded_payment_id IN qr_payments.refunded_payment_id%TYPE,
        p_refunded_unique_id IN qr_payments.refunded_unique_id%TYPE,
        p_refunded_end_to_end_id IN qr_payments.refunded_end_to_end_id%TYPE,
        p_ips_status IN qr_payments.ips_status%TYPE,
        p_is_canceled IN qr_payments.is_canceled%TYPE,
        p_description IN qr_payments.description%TYPE,
        p_id OUT qr_payments.id%TYPE) AS
    BEGIN
        p_id := qr_payments_seq.NEXTVAL;

        INSERT INTO qr_payments
        (
            id,
            unique_id,
            end_to_end_id,
            serial_number,
            payment_type,
            total_amount,
            qr_code_str,
            status_code,
            status_desc,
            merchant_no,
            terminal_no,
            currency_code,
            refunded_payment_id,
            refunded_unique_id,
            refunded_end_to_end_id,
            ips_status,
            is_canceled,
            description
        )
        VALUES
        (
            p_id,
            p_unique_id,
            p_end_to_end_id,
            p_serial_number,
            p_payment_type,
            p_total_amount,
            p_qr_code_str,
            p_status_code,
            p_status_desc,
            p_merchant_no,
            p_terminal_no,
            p_currency_code,
            p_refunded_payment_id,
            p_refunded_unique_id,
            p_refunded_end_to_end_id,
            p_ips_status,
            p_is_canceled,
            p_description
        );
    END insert_qr_payment;

    PROCEDURE get_qr_payment_by_unique_id(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_result OUT SYS_REFCURSOR) AS
    BEGIN
        OPEN p_result FOR
            SELECT
                id AS "Id",
                unique_id AS "UniqueId",
                end_to_end_id AS "EndToEndId",
                serial_number AS "SerialNumber",
                payment_type AS "PaymentType",
                total_amount AS "TotalAmount",
                qr_code_str AS "QrCodeStr",
                status_code AS "StatusCode",
                status_desc AS "StatusDesc",
                merchant_no AS "MerchantNo",
                terminal_no AS "TerminalNo",
                currency_code AS "CurrencyCode",
                refunded_payment_id AS "RefundedPaymentId",
                refunded_unique_id AS "RefundedUniqueId",
                refunded_end_to_end_id AS "RefundedEndToEndId",
                ips_status AS "IpsStatus",
                is_canceled AS "IsCanceled",
                description AS "Description"
            FROM qr_payments
            WHERE unique_id = p_unique_id
              AND ROWNUM = 1;
    END get_qr_payment_by_unique_id;

    PROCEDURE update_qr_payment_status(
        p_unique_id IN qr_payments.unique_id%TYPE,
        p_status_code IN qr_payments.status_code%TYPE,
        p_status_desc IN qr_payments.status_desc%TYPE,
        p_is_canceled IN qr_payments.is_canceled%TYPE,
        p_ips_status IN qr_payments.ips_status%TYPE,
        p_description IN qr_payments.description%TYPE) AS
    BEGIN
        UPDATE qr_payments
        SET status_code = p_status_code,
            status_desc = p_status_desc,
            is_canceled = p_is_canceled,
            ips_status = p_ips_status,
            description = p_description,
            updated_at_utc = SYS_EXTRACT_UTC(SYSTIMESTAMP)
        WHERE unique_id = p_unique_id;
    END update_qr_payment_status;

    PROCEDURE seed_sample_terminal AS
        v_exists NUMBER(1);
    BEGIN
        SELECT COUNT(1)
        INTO v_exists
        FROM terminals
        WHERE serial_number = 'V1E0230675'
          AND ROWNUM = 1;

        IF v_exists = 0 THEN
            INSERT INTO terminals
            (
                id,
                serial_number,
                terminal_no,
                terminal_model,
                terminal_type,
                country_code,
                header_country_code,
                currency_code,
                currency_numeric_code,
                terminal_language_code,
                receipt_language_code,
                time_zone,
                rrn_prefix,
                company_id,
                company_code,
                company_name,
                merchant_id,
                register_id,
                register_tsm_id,
                merchant_name,
                merchant_address1,
                phone_number,
                category_code,
                merchant_no,
                email,
                tax_number,
                city,
                postal_code,
                branch_name,
                alias_type,
                alias_value,
                bank_bic,
                provider_bic,
                operation_code,
                transaction_type,
                ips_spec_version,
                ips_uuid,
                delivery_channel,
                coordinates
            )
            VALUES
            (
                terminals_seq.NEXTVAL,
                'V1E0230675',
                'NBA12345',
                'X990',
                '02',
                'AZ',
                'AZE',
                'AZN',
                '944',
                'az',
                'az',
                'Asia/Baku',
                'NBA',
                3,
                'NBA',
                'NBA',
                5,
                32,
                HEXTORAW(REPLACE('29526D61-933F-440F-A2EF-ADEF8BDD4F39', '-', '')),
                'CAFE CITY BAKU',
                'Nizami str. 65',
                '+994503595558',
                '5812',
                '11205228',
                '',
                '1001812792',
                'BAKU',
                'AZ1014',
                'CAFE CITY BAKU',
                '04',
                'AZ49IBAZ40050019449333061204',
                'IBAZAZ20',
                'NBATAZ20',
                'MPRQ-ATP',
                '613',
                'MPV002',
                '8779c7cfceb149b89546c4f3faea3721',
                '400',
                NULL
            );
        END IF;
    END seed_sample_terminal;
END nba_qr_api_pkg;
/
