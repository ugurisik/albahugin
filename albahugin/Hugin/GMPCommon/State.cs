namespace Hugin.GMPCommon { 

public enum State
{
	ST_IDLE = 1,
	ST_SELLING,
	ST_SUBTOTAL,
	ST_PAYMENT,
	ST_OPEN_SALE,
	ST_INFO_RCPT,
	ST_CUSTOM_RCPT,
	ST_IN_SERVICE,
	ST_SRV_REQUIRED,
	ST_LOGIN,
	ST_NONFISCAL,
	ST_ON_PWR_RCOVR,
	ST_INVOICE,
	ST_CONFIRM_REQUIRED,
	ST_ECR_ON_USE
}
}