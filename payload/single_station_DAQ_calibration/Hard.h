// Hard.h: interface for the CHard class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_HARD_H__0825ABE2_BBFA_4220_9826_6FCBBDB4BA92__INCLUDED_)
#define AFX_HARD_H__0825ABE2_BBFA_4220_9826_6FCBBDB4BA92__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CHard  
{
public:
	CHard();
	virtual ~CHard();
//Attributes
public:
	USHORT m_nDeviceIndex;
	USHORT m_nDeviceNum;
	//USHORT m_nComType;//0:USB; 1:LAN
	short* m_pSrcData[MAX_CH_NUM];//读取的数据减去零电平的位置(-255 ~ 255)
	//USHORT m_nCalLevel[CAL_LEVEL_LEN];//Cal Level
	USHORT m_nTimeDIV;
	USHORT m_nYTFormat;
	BOOL m_bCollect;
	CONTROLDATA m_stControl;
	RELAYCONTROL RelayControl;
	USHORT m_nTriggerMode;
	USHORT m_nTriggerSweep;
	USHORT m_nTriggerSlope;
	USHORT m_nLeverPos[MAX_CH_NUM];
	COLORREF m_clrRGB[MAX_CH_NUM];
	FILE* pulselastfile;
	int HantekId;
	//WORD pAmpLevel[AMPCALI_Len];
	//int iev[MAX_CH_NUM] = { 0,0,0,0 };
	int m_nReadOK;//本次读数据是否正确,0,不正确；非0不正确。
//Operations
public:
	double pm[5000];
	double data[BUF_4K_LEN];
	double pulse[BUF_4K_LEN][MAX_CH_NUM];
	int default = CH2;
	bool FindeDev();
	void Init();
	int evt_offset;
	void ReadData();
	double amaxall_last;
	void SaveData();
	USHORT* pReadData[MAX_CH_NUM];
	double spm[BUF_4K_LEN][MAX_CH_NUM][4];
	float npm[MAX_CH_NUM][4] = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
	void ReadSCANData();
	void SourceToDisplay(USHORT* pData,ULONG nDataLen,USHORT nCH,int nOffset=0);
	float trgLevel[MAX_CH_NUM] = {0.0,0.0, 0.0, 0.0};
	void ChangeTrigger();
	void DoChangeTrigger();
	bool change_trigger;
};

#endif // !defined(AFX_HARD_H__0825ABE2_BBFA_4220_9826_6FCBBDB4BA92__INCLUDED_)
