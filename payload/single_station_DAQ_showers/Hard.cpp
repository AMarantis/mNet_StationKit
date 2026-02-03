// Hard.cpp: implementation of the CHard class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "VCDSO.h"
#include "Hard.h"
#include "timeapi.h"
//#include "sysinfoapi.h"
#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CHard::CHard()
{
	HantekId = 0;//0 or the 1st device, 1 for the second etc
	evt_offset = -1;
	change_trigger = true;
	m_stControl.nTriggerSource = CH1;//Í¨µÀ1Îª´¥·¢Í¨µÀ
	default = 0; //this means that the trigger channel in CH0 (see Init)
	trgLevel[CH1] = 5.0;
	trgLevel[CH2] = 5.0;
	trgLevel[CH3] = 5.0;
	trgLevel[CH4] = 5.0;

	m_nLeverPos[CH1] = 230;// 192;
	m_nLeverPos[CH2] = 230;
	m_nLeverPos[CH3] = 230;
	m_nLeverPos[CH4] = 230;
	ULONG i = 0;
	m_nDeviceIndex = 0xFF;
	m_nDeviceNum = 0;
	for (i = 0; i < MAX_CH_NUM; i++)
	{
		m_pSrcData[i] = new short[BUF_4K_LEN];
	}
	m_clrRGB[CH3] = RGB(255, 0, 0);
	m_clrRGB[CH1] = RGB(0, 0, 255);
	m_clrRGB[CH2] = RGB(0, 255, 0);
	m_clrRGB[CH4] = RGB(0, 255, 255);
	m_nTimeDIV = 0;//24;

	m_stControl.nCHSet = 0x0F;//ËùÓÐÍ¨µÀ´ò¿ª
	m_stControl.nTimeDIV = m_nTimeDIV;//Factory Setup

	m_stControl.nVTriggerPos = m_nLeverPos[CH1] - 16;//set to 16* 80/255 = 5.02 the trigger threshold
	m_stControl.nTriggerSlope = FALL;//±ßÑØ´¥·¢µÄ´¥·¢·½Ê½£ºÉÏÉýÑØ
	m_stControl.nBufferLen = BUF_4K_LEN;//²É¼¯Éî¶È
	m_stControl.nReadDataLen = BUF_4K_LEN;//¶ÁÈ¡³¤¶È<=²É¼¯Éî¶È
	m_stControl.nLastAddress = 0;  //add by zhang
	m_stControl.nAlreadyReadLen = 0;//Ö»ÔÚÉ¨Ãè¹ö¶¯Çé¿öÏÂÓÐÐ§£¬ÓÃÀ´¼ÇÂ¼ÒÑ¾­¶ÁÈ¡µÄ³¤¶È
	m_stControl.nALT = 0;//Factory Setup

	m_nYTFormat = m_nTimeDIV > 23 ? YT_SCAN : YT_NORMAL;
	m_stControl.nHTriggerPos = m_nYTFormat == YT_SCAN ? 0 : 20;//sto proto 20% toy xtonikoy eyroys 
	for (i = 0; i < MAX_CH_NUM; i++)
	{
		RelayControl.bCHEnable[i] = 1;
		RelayControl.nCHVoltDIV[i] = 2;
		RelayControl.nCHCoupling[i] = AC;
		RelayControl.bCHBWLimit[i] = 0;
	}
	RelayControl.nTrigSource = CH1;
	RelayControl.bTrigFilt = 0;
	RelayControl.nALT = 0;
	m_nTriggerMode = EDGE;
	m_nTriggerSlope = FALL;
	m_nTriggerSweep = SINGLE;

	m_bCollect = FALSE;
	m_nReadOK = 0;


}

CHard::~CHard()
{
}

void CHard::Init()
{
	if (change_trigger) m_stControl.nTriggerSource = default;// (default + 1) % 4;// just put the trigger source to other channel
	dsoInitHard(m_nDeviceIndex);//Ó²¼þ³õÊ¼»¯
	dsoHTADCCHModGain(m_nDeviceIndex, 4);//ÉèÖÃÓÉÍ¨µÀÄ£Ê½ÒýÆðµÄ·ù¶ÈÐÞÕý
	// place here test code
	//dsoHTSetCHAndTrigger(m_nDeviceIndex, &RelayControl, 12);
	//
	//return;

	dsoHTSetSampleRate(m_nDeviceIndex, m_nYTFormat, &RelayControl, &m_stControl);//ÉèÖÃ²ÉÑùÂÊ
	dsoHTSetCHAndTrigger(m_nDeviceIndex, &RelayControl, m_stControl.nTimeDIV);//ÉèÖÃÍ¨µÀ¿ª¹ØºÍµçÑ¹µµÎ»
	//return;
	dsoHTSetRamAndTrigerControl(m_nDeviceIndex, m_stControl.nTimeDIV, m_stControl.nCHSet, m_stControl.nTriggerSource, 0);//ÉèÖÃ´¥·¢Ô´
	for (int i = 0; i < MAX_CH_NUM; i++)
	{
		dsoHTSetCHPos(m_nDeviceIndex, RelayControl.nCHVoltDIV[i], m_nLeverPos[i], i, 4);
	}
	dsoHTSetVTriggerLevel(m_nDeviceIndex, m_stControl.nVTriggerPos, 4);
	//	starttime[default] = GetTickCount();//notthis
	switch (m_nTriggerMode) {//´¥·¢ÉèÖÃ
	case EDGE:
		dsoHTSetTrigerMode(m_nDeviceIndex, m_nTriggerMode, m_stControl.nTriggerSlope, DC);
		break;
		/*
	case VIDEO:
		{
		double dbVolt=m_dbVoltDIV[RelayControl.nCHVoltDIV[m_nALTSelCH]];
		short nPositive=nVideoPositive==POSITIVE?TRIGGER_VIDEO_POSITIVE:TRIGGER_VIDEO_NEGATIVE;
		WORD nTriggerLevel=255-GetCHLeverPos(m_nALTSelCH)+short((256*nPositive)/(dbVolt*V_GRID_NUM)+0.5);
		dsoHTSetTrigerMode(m_nDeviceIndex,m_nTriggerMode,m_stControl.nTriggerSlope,m_Trigger.m_nTriggerCouple);
		dsoHTSetVideoTriger(m_nDeviceIndex,nVideoStandard,nVideoSyncSelect,nVideoHsyncNumOption,nVideoPositive,nTriggerLevel,GetLogicTriggerSource(m_nALTSelCH));
		break;}
	case PULSE:
		dsoHTSetTrigerMode(m_nDeviceIndex,m_nTriggerMode,m_stControl.nTriggerSlope,m_Trigger.m_nTriggerCouple);
		dsoHTSetPulseTriger(m_nDeviceIndex,nPW,nPWCondition);
		break;
	case FORCE:
		dsoHTSetTrigerMode(m_nDeviceIndex,m_nTriggerMode,m_stControl.nTriggerSlope,m_Trigger.m_nTriggerCouple);
		*/
	default:
		break;
	}

}
bool CHard::FindeDev()//καλειται στον timer1 και οταν το m_nDeviceIndex=0xFF
{
	for (m_nDeviceIndex = HantekId; m_nDeviceIndex < 32; m_nDeviceIndex++)
	{
		if (dsoHTDeviceConnect(m_nDeviceIndex))
		{
			WORD ttt = dsoGetFPGAVersion(m_nDeviceIndex);//3-61445,0-61444,1-61445,2-61445
			Init();
			return true;
		}
	}
	m_nDeviceIndex = 0xFF;
	return false;

}

void CHard::SaveData()
{
	double avg[4] = { 0.0,0.0,0.0,0.0 };
	double amax[4] = { 0.0,0.0,0.0,0.0 };
	int ktr = 9999;
	int alltriggers = 0;
	for (int mych = 0; mych < 4; mych++)
	{
		SourceToDisplay(pReadData[mych], m_stControl.nReadDataLen, mych);//ÎªÁË·½±ãÏÔÊ¾
		for (int k = 0; k < 200; k++)
		{
			double VertScale = 0.080 / 255.0;
			double vv = -1000.0 * VertScale * (float(pReadData[mych][k]) - float(m_nLeverPos[mych]));
			avg[mych] += vv / 200.;
		}
		for (int k = 0; k < 4096; k++)
		{
			double VertScale = 0.080 / 255.0;
			double vv = -1000.0 * VertScale * (float(pReadData[mych][k]) - float(m_nLeverPos[mych]));
			if (fabs(vv - avg[mych]) > fabs(amax[mych])) amax[mych] = vv - avg[mych];
			if (k<ktr && fabs(vv - avg[mych]) > trgLevel[mych]) ktr = k;
			pulse[k][mych] = vv - avg[mych];
		}
		if (amax[mych] > trgLevel[mych])alltriggers++;
	}

	if (alltriggers > 2)
	{
		if (default == 3)
		{
			CString ss = "1";
		}
		if (evt_offset == -1)//always
		{
			SYSTEMTIME st, lt;
			//GetSystemTime(&st);
			GetLocalTime(&lt);
			CString filename; filename.Format("%d_%d_%d_%d_%d.showerdata", HantekId + 1, lt.wYear, lt.wMonth, lt.wDay, lt.wHour);
			pulselastfile = fopen(filename, "a");
			fprintf(pulselastfile, "%5.1f %5.1f %5.1f %5.1f\n", -99.0, -99.0, -99.0, -99.0);
			fprintf(pulselastfile, "%2d %2d %3d\n", lt.wMinute, lt.wSecond, lt.wMilliseconds);
			//fseek(pulselastfile, 0, SEEK_END);
			//evt_offset = ftell(pulselastfile) / 41;
		}
		//evt_offset++;
		float zero = 0.0;
		//		fprintf(pulselastfile, "%10d %2d %2d %2d %4.1f %4.1f %4.1f %4.1f\n", default, lt.wHour, lt.wMinute, lt.wSecond, amax[CH1], amax[CH2], amax[CH3], amax[CH4]);//41 bytes per event
		if (fabs(amax[CH1]) < 100 && fabs(amax[CH2]) < 100 && fabs(amax[CH3] < 100) && fabs(amax[CH4]) < 100)
			fprintf(pulselastfile, "%5.1f %5.1f %5.1f %5.1f\n", amax[CH1], amax[CH2], amax[CH3], amax[CH4]);//21 bytes per event
		else
			fprintf(pulselastfile, "%5.1f %5.1f %5.1f %5.1f\n", zero, zero, zero, zero);//21 bytes per event

		/*
		typedef struct _SYSTEMTIME {
			WORD wYear;
			WORD wMonth;
			WORD wDayOfWeek;
			WORD wDay;
			WORD wHour;
			WORD wMinute;
			WORD wSecond;
			WORD wMilliseconds;
		} SYSTEMTIME, * PSYSTEMTIME, * LPSYSTEMTIME;
		*/
/*
		double amax1[4] = { 0.0,0.0,0.0,0.0 };
		bool ok1 = false;
		bool ok2 = false;
		bool ok3 = false;
		bool ok4 = false;
		*/
		for (int k = ktr - 50; k < ktr + 150; k++) //200*21=3150 bytes 86016 bytes per event
		{
			/*
			if (abs(pulse[k][CH1]) > amax1[CH1])amax1[CH1] = abs(pulse[k][CH1]);
			if (abs(pulse[k][CH2]) > amax1[CH2])amax1[CH2] = abs(pulse[k][CH2]);
			if (abs(pulse[k][CH3]) > amax1[CH3])amax1[CH3] = abs(pulse[k][CH3]);
			if (abs(pulse[k][CH4]) > amax1[CH4])amax1[CH4] = abs(pulse[k][CH4]);
			if (amax[CH1] == amax1[CH1])ok1 = true;
			if (amax[CH2] == amax1[CH2])ok2 = true;
			if (amax[CH3] == amax1[CH3])ok3 = true;
			if (amax[CH4] == amax1[CH4])ok4 = true;
			*/
			if (fabs(pulse[k][CH1]) < 100 && fabs(pulse[k][CH2]) < 100 && fabs(pulse[k][CH3] < 100) && fabs(pulse[k][CH4]) < 100)
				fprintf(pulselastfile, "%5.1f %5.1f %5.1f %5.1f\n", pulse[k][CH1], pulse[k][CH2], pulse[k][CH3], pulse[k][CH4]);
			else
				fprintf(pulselastfile, "%5.1f %5.1f %5.1f %5.1f\n", zero, zero, zero, zero);//21 bytes per event
		}
		/*
		double xx, yy, hh;
		if (!ok1)
		{
			xx = amax[CH1];
            yy = amax1[CH1];
			hh = xx - yy;
		}
		if (!ok2)
		{
			xx = amax[CH2];
			yy = amax1[CH2];
			hh = xx - yy;
		}
		if (!ok3)
		{
			xx = amax[CH3];
			yy = amax1[CH3];
			hh = xx - yy;
		}
		if (!ok4)
		{
			xx = amax[CH4];
			yy = amax1[CH4];
			hh = xx - yy;
		}
		hh = xx - yy;
		*/
		fclose(pulselastfile);
		//exit(0);
		//4221 bytes per event
	}
}
//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////



void CHard::ChangeTrigger() //αυτο γινεται στο timer2
{
	//change_trigger = true;//notthis
	DoChangeTrigger();

}
void CHard::DoChangeTrigger()
{

	default = (default + 1) % 4; //if (default == 3)default = 0;
	Init();


}

void CHard::ReadData()
{
	int i = 0;
	//USHORT* pReadData[MAX_CH_NUM];
	for (i = 0; i < MAX_CH_NUM; i++)
	{
		pReadData[i] = new USHORT[m_stControl.nReadDataLen];
		memset(pReadData[i], 0, m_stControl.nReadDataLen * sizeof(USHORT));//
	}
	m_nReadOK = dsoHTGetData(m_nDeviceIndex, pReadData[CH1], pReadData[CH2], pReadData[CH3], pReadData[CH4], &m_stControl);//Ã¿Í¨µÀiµÚj¸öµãµÄÊµ¼ÊµçÑ¹Öµ=(pReadData[i][j]-m_nLeverPos[i])*8*µçÑ¹Öµ/255
	if (m_nReadOK == 1)
	{
		SaveData();//change this
	}
	if (change_trigger)
		//if (iev[default] % 100 == 0)
	{
		//DoChangeTrigger();
	}

	for (i = 0; i < MAX_CH_NUM; i++)
	{
		delete pReadData[i];
	}
}
void CHard::ReadSCANData()
{
	int i = 0;
	USHORT* pReadData[MAX_CH_NUM];
	for (i = 0; i < MAX_CH_NUM; i++)
	{
		pReadData[i] = new USHORT[m_stControl.nReadDataLen];
		memset(pReadData[i], 0, m_stControl.nReadDataLen * sizeof(USHORT));//
	}
	int nLastSCANLen = m_stControl.nAlreadyReadLen;
	m_nReadOK = dsoHTGetScanData(m_nDeviceIndex, pReadData[CH1], pReadData[CH2], pReadData[CH3], pReadData[CH4], &m_stControl);//Ã¿Í¨µÀiµÚj¸öµãµÄÊµ¼ÊµçÑ¹Öµ=(pReadData[i][j]-m_nLeverPos[i])*8*µçÑ¹Öµ/255
	int nCurSCANLen = m_stControl.nAlreadyReadLen;
	int nCurReadLen = nCurSCANLen - nLastSCANLen;
	CString str;
	str.Format(("%d %d\n"), nCurReadLen, nCurSCANLen);
	OutputDebugString(str);
	if (m_nReadOK && nCurSCANLen > nLastSCANLen)
	{
		for (i = 0; i < MAX_CH_NUM; i++)
		{
			SourceToDisplay(pReadData[i], nCurReadLen, i, nLastSCANLen);//ÎªÁË·½±ãÏÔÊ¾
		}
	}

	for (i = 0; i < MAX_CH_NUM; i++)
	{
		delete pReadData[i];
	}

}
void CHard::SourceToDisplay(USHORT* pData, ULONG nDataLen, USHORT nCH, int nOffset)
{
	for (ULONG i = 0; i < nDataLen; i++)
	{
		*(m_pSrcData[nCH] + i + nOffset) = *(pData + i) - (MAX_DATA - m_nLeverPos[nCH]);
	}
}