-- Oracle 11g schema for NBA AzQR terminal and payment storage.
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE qr_payments CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE terminals CASCADE CONSTRAINTS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP SEQUENCE qr_payments_seq';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -2289 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP SEQUENCE terminals_seq';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -2289 THEN
            RAISE;
        END IF;
END;
/

CREATE TABLE terminals
(
    id                      NUMBER(10)    NOT NULL,
    serial_number           NVARCHAR2(50) NOT NULL,
    terminal_no             NVARCHAR2(25) NOT NULL,
    terminal_model          NVARCHAR2(50),
    terminal_type           CHAR(2)       DEFAULT '02' NOT NULL,
    country_code            CHAR(2)       DEFAULT 'AZ' NOT NULL,
    header_country_code     NVARCHAR2(3)  DEFAULT 'AZE' NOT NULL,
    currency_code           CHAR(3)       DEFAULT 'AZN' NOT NULL,
    currency_numeric_code   CHAR(3)       DEFAULT '944' NOT NULL,
    terminal_language_code  NVARCHAR2(5)  DEFAULT 'az' NOT NULL,
    receipt_language_code   NVARCHAR2(5)  DEFAULT 'az' NOT NULL,
    time_zone               NVARCHAR2(50) DEFAULT 'Asia/Baku' NOT NULL,
    rrn_prefix              NVARCHAR2(10) NOT NULL,

    company_id              NUMBER(10),
    company_code            NVARCHAR2(20)  NOT NULL,
    company_name            NVARCHAR2(100) NOT NULL,
    merchant_id             NUMBER(10),
    register_id             NUMBER(10),
    register_tsm_id         RAW(16),

    merchant_name           NVARCHAR2(25)  NOT NULL,
    merchant_address1       NVARCHAR2(200),
    phone_number            NVARCHAR2(30),
    category_code           CHAR(4)        NOT NULL,
    merchant_no             NVARCHAR2(35)  NOT NULL,
    email                   NVARCHAR2(100),
    tax_number              NVARCHAR2(10),
    city                    NVARCHAR2(15)  NOT NULL,
    postal_code             NVARCHAR2(10),
    branch_name             NVARCHAR2(25)  NOT NULL,

    alias_type              CHAR(2)        DEFAULT '04' NOT NULL,
    alias_value             NVARCHAR2(35)  NOT NULL,
    bank_bic                NVARCHAR2(11),
    provider_bic            NVARCHAR2(11)  NOT NULL,
    operation_code          NVARCHAR2(10)  DEFAULT 'MPRQ-ATP' NOT NULL,
    transaction_type        CHAR(3)        DEFAULT '613' NOT NULL,
    ips_spec_version        CHAR(6)        DEFAULT 'MPV002' NOT NULL,
    ips_uuid                CHAR(32)       NOT NULL,
    delivery_channel        CHAR(3)        DEFAULT '400' NOT NULL,
    coordinates             CHAR(16),
    consumer_info_query     NVARCHAR2(3),
    tip_fee_type            CHAR(2),
    fixed_convenience_fee   NVARCHAR2(13),
    convenience_fee_percent NVARCHAR2(5),
    alt_language_code       CHAR(2),
    alt_merchant_name       NVARCHAR2(25),
    alt_city                NVARCHAR2(15),

    is_active               NUMBER(1)      DEFAULT 1 NOT NULL,
    created_at_utc          TIMESTAMP(3)   DEFAULT SYS_EXTRACT_UTC(SYSTIMESTAMP) NOT NULL,
    updated_at_utc          TIMESTAMP(3)   DEFAULT SYS_EXTRACT_UTC(SYSTIMESTAMP) NOT NULL,

    CONSTRAINT pk_terminals PRIMARY KEY (id),
    CONSTRAINT uq_terminals_serial_number UNIQUE (serial_number),
    CONSTRAINT ck_terminals_is_active CHECK (is_active IN (0, 1))
);

CREATE TABLE qr_payments
(
    id                      NUMBER(19)      NOT NULL,
    unique_id               NVARCHAR2(32)   NOT NULL,
    end_to_end_id           NVARCHAR2(32)   NOT NULL,
    serial_number           NVARCHAR2(50)   NOT NULL,
    payment_type            NUMBER(10)      NOT NULL,
    total_amount            NUMBER(13, 2)   NOT NULL,
    qr_code_str             CLOB            NOT NULL,
    status_code             NUMBER(10)      NOT NULL,
    status_desc             NVARCHAR2(200)  NOT NULL,
    merchant_no             NVARCHAR2(35)   NOT NULL,
    terminal_no             NVARCHAR2(25)   NOT NULL,
    currency_code           CHAR(3)         NOT NULL,
    refunded_payment_id     NUMBER(19),
    refunded_unique_id      NVARCHAR2(32),
    refunded_end_to_end_id  NVARCHAR2(32),
    ips_status              NVARCHAR2(50),
    is_canceled             NUMBER(1)       DEFAULT 0 NOT NULL,
    description             NVARCHAR2(200),
    created_at_utc          TIMESTAMP(3)    DEFAULT SYS_EXTRACT_UTC(SYSTIMESTAMP) NOT NULL,
    updated_at_utc          TIMESTAMP(3)    DEFAULT SYS_EXTRACT_UTC(SYSTIMESTAMP) NOT NULL,

    CONSTRAINT pk_qr_payments PRIMARY KEY (id),
    CONSTRAINT uq_qr_payments_unique_id UNIQUE (unique_id),
    CONSTRAINT fk_qr_payments_terminals FOREIGN KEY (serial_number) REFERENCES terminals (serial_number),
    CONSTRAINT ck_qr_payments_is_canceled CHECK (is_canceled IN (0, 1))
);

CREATE INDEX ix_qr_payments_serial ON qr_payments (serial_number, created_at_utc DESC);

CREATE SEQUENCE terminals_seq START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;

CREATE SEQUENCE qr_payments_seq START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
