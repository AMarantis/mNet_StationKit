void pulses()
{
	auto	hvv1 = new  TH1F("Stats", "Peak Voltage 1", 15, 0.0, 90.0);  // Create a 1D histogram object of floats
	auto	hvv2 = new  TH1F("Stats", "Peak Voltage 2", 15, 0.0, 90.0);  // Create a 1D histogram object of floats
	auto	hvv3 = new  TH1F("Stats", "Peak Voltage 3", 15, 0.0, 90.0);  // Create a 1D histogram object of floats
	auto	hvv4 = new  TH1F("Stats", "T1-T3", 20, -50.0, 50.0);  // Create a 1D histogram object of floats
	auto	hvv5 = new  TH1F("Stats", "T2-T3", 20, -50.0, 50.0);  // Create a 1D histogram object of floats
	auto	hvv6 = new  TH1F("Stats", "T1-T2", 20, -50.0, 50.0);  // Create a 1D histogram object of floats
	auto	hvv7 = new  TH1F("Stats", "DT", 20, 0.0, 30.0);  // Create a 1D histogram object of floats
	auto   hvv8 = new  TH1F("Stats", "Zenith angle", 10, 0.0, 90.0);  // Create a 1D histogram object of floats
	auto   hvv9 = new  TH1F("Stats", "Azimuth angle", 10, 0.0, 360.0);  // Create a 1D histogram object of floats

	auto c1 = new TCanvas("c1","Candle Decay",1500,300);
	c1->SetFillColor(204);
	gStyle->SetStatStyle(204);

 float vol1,vol2,vol3;
 //float time,v0,v1,v2;
 int i=0;
 int ii=0;
 float pulse1[200];
 float pulse2[200];
 float pulse3[200];
 float x[200];//,y1[200];

 FILE* inp1=fopen("events.txt","r");
 float amax1=0.0;
 float amax2=0.0;
 float amax3=0.0;
 
 int year,month,day,hour,min,sec,msec;
 float t1,t2,t3,p1,p2,p3,ch1,ch2,ch3,th,ph,thr,phr;
if(inp1==0)return;
 float prevv = -1;
 float tt;
 cout << "gggg" << endl;
 while(! feof(inp1))
 {
	fscanf(inp1,"%d %d %d %d %d %d %d\n",&year,&month,&day,&hour,&min,&sec,&msec);	ii++;
	fscanf(inp1,"%f %f %f %f %f %f %f %f %f %f %f %f %f\n",&t1,&t2,&t3,&p1,&p2,&p3,&ch1,&ch2,&ch3,&th,&ph, &thr, &phr);	ii++;
							 if (prevv > -1)tt = (float)(min) * 60 + (float)sec - prevv; else tt = -1;
							 prevv = (float)(min) * 60 + (float)sec;

   if (!(thr < 0 && phr < 0 && th < 0 && ph < 0))
   {
	   hvv1->Fill(p1);
	   hvv2->Fill(p2);
	   hvv3->Fill(p3);
	   hvv4->Fill(t1 - t3);
	   hvv5->Fill(t2 - t3);
	   hvv6->Fill(t1 - t2);
	   hvv7->Fill(tt / 60);
	   hvv8->Fill(th);
	   hvv9->Fill(ph);
   }
	for (int i=0;i<200;i++)
	{		
	fscanf(inp1,"%f %f %f\n",&vol1, &vol2, &vol3);
     pulse1[i]=vol1;
     pulse2[i]=vol2;
     pulse3[i]=vol3;
	 x[i]=float(i)*4.0;
	 //cout<<vol1<<" "<<vol2<<" "<<vol3<<endl;
	 if(vol1>amax1)amax1=vol1;
	 if(vol2>amax2)amax2=vol2;
	 if(vol3>amax3)amax3=vol3;
	ii++;
	}
 }
 fclose(inp1);
 /*
 for (int k=0;k<200;k++)
 {
	 cout<<pulse1[k]<<" "<<pulse2[k]<<" "<<pulse3[k]<<endl;
 }
 */
 cout<<ii<<" ------ "<<ii/202<<endl;

TGraph* gr1= new TGraph(200,x,pulse1);
TGraph* gr2= new TGraph(200,x,pulse2);
TGraph* gr3= new TGraph(200,x,pulse3);

 cout<<"first part completed OK"<<endl;

 auto	hv1 = new  TH1F("Stats", "Peak Voltage 1", 15, 0.0, 90.0);  // Create a 1D histogram object of floats
 auto	hv2 = new  TH1F("Stats", "Peak Voltage 2", 15, 0.0, 90.0);  // Create a 1D histogram object of floats
 auto	hv3 = new  TH1F("Stats", "Peak Voltage 3", 15, 0.0, 90.0);  // Create a 1D histogram object of floats
 auto	hv4 = new  TH1F("Stats", "T1-T3", 20, -50.0, 50.0);  // Create a 1D histogram object of floats
 auto	hv5 = new  TH1F("Stats", "T2-T3", 20, -50.0, 50.0);  // Create a 1D histogram object of floats
 auto	hv6 = new  TH1F("Stats", "T1-T2", 20, -50.0, 50.0);  // Create a 1D histogram object of floats
 auto	hv7 = new  TH1F("Stats", "DT", 20, 0.0, 30.0);  // Create a 1D histogram object of floats
 auto   hv8 = new  TH1F("Stats", "Zenith angle", 10, 0.0, 90.0);  // Create a 1D histogram object of floats
 auto   hv9 = new  TH1F("Stats", "Azimuth angle", 10, 0.0, 360.0);  // Create a 1D histogram object of floats

 FILE* ref1 = fopen("reference_shower_start.txt", "r");
 FILE* ref2 = fopen("reference_shower_end.txt", "r");
 int year1, month1, day1;
 int year2, month2, day2;
 int station;
 if (ref1 != 0)
 {
	 fscanf(ref1, "%d\n", &station);
	 fscanf(ref1, "%d\n", &year1);
	 fscanf(ref1, "%d\n", &month1);
	 fscanf(ref1, "%d\n", &day1);
 }
 else
 {
		cout<<"ref1=0"<<endl;
 }
 if (ref2 != 0)
 {
	 fscanf(ref2, "%d\n", &station);
	 fscanf(ref2, "%d\n", &year2);
	 fscanf(ref2, "%d\n", &month2);
	 fscanf(ref2, "%d\n", &day2);
 }
else
 {
		cout<<"ref2=0"<<endl;
 }
 
 if (ref1 != 0 && ref2 != 0)
 {

	 cout << "scanning from " << day1 << "/" << month1 << "/" << year1 << " to " << day2 << "/" << month2 << "/" << year2 << endl;

	 int yeari, monthi, dayi, houri, mini, seci, mseci;
	 float t1i, t2i, t3i, p1i, p2i, p3i, ch1i, ch2i, ch3i, thi, phi, thri, phri;
	 float vol1i, vol2i, vol3i;

	 year = year1;
	 month = month1;
	 day = day1;
	 bool finished = false;
	 while (!finished)
	 {
		 //open file etc
		 for (int hour = 0; hour < 24; hour++)
		 {
			 char buffer[150];
			 //sprintf(buffer, "D:\\Save_Pulses_Calibration\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
			 sprintf(buffer, "D:\\Save_Pulses_Showers_Rec\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
			 FILE* datafile = fopen(buffer, "r");
			 //						 cout << "trying opening file " << buffer << " " << year << " " << month << " " << day << " " << hour << endl; //getchar();
			 if (datafile != 0)
			 {
				 cout << "opening file " << buffer << " " << year << " " << month << " " << day << " " << hour << endl; //getchar();
				 float prev = -1;
				 while (!feof(datafile))
				 {
					 fscanf(datafile, "%d %d %d %d %d %d %d\n", &yeari, &monthi, &dayi, &houri, &mini, &seci, &mseci);
					 if (prev > -1)tt = (float)(mini) * 60 + (float)seci - prev; else tt = -1;
					 prev = (float)(mini) * 60 + (float)seci;
					 fscanf(datafile, "%f %f %f %f %f %f %f %f %f %f %f %f %f\n", &t1i, &t2i, &t3i, &p1i, &p2i, &p3i, &ch1i, &ch2i, &ch3i, &thi, &phi, &thri, &phri);
					 for (int i = 0; i < 200; i++)
						 fscanf(datafile, "%f %f %f\n", &vol1i, &vol2i, &vol3i);
					 if (!(thri < 0 && phri < 0 && thi < 0 && phi < 0))
					 {
						 hv1->Fill(p1i);
						 hv2->Fill(p2i);
						 hv3->Fill(p3i);
						 hv4->Fill(t1i - t3i);
						 hv5->Fill(t2i - t3i);
						 hv6->Fill(t1i - t2i);
						 hv7->Fill(tt / 60);
						 hv8->Fill(thi);
						 hv9->Fill(phi);

					 }
				 }
				 fclose(datafile);
			 }
		 }
		 if (year == year2 && month == month2 && day == day2) finished = true;
		 day++;
		 if (day > 31){day = 1; month++;}
		 if (month > 12) {month = 1; year++;}
	 }

	 /*
	 for (int year = year1; year < year2+1; year++)
	 {
		 for (int month = month1+1; month < month2; month++)
		 {
			 for (int day = 1; day < 32; day++)
			 {
				 for (int hour = 0; hour < 24; hour++)
				 {
					 char buffer[150];
					 //sprintf(buffer, "D:\\Save_Pulses_Calibration\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
					 sprintf(buffer, "D:\\Save_Pulses_Showers_Rec\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
					 FILE* datafile = fopen(buffer, "r");
//						 cout << "trying opening file " << buffer << " " << year << " " << month << " " << day << " " << hour << endl; //getchar();
					 if (datafile != 0)
					 {
						 cout << "opening file " << buffer << " " << year << " " << month << " " << day << " " << hour << endl; //getchar();
						 float prev = -1;
						 while (!feof(datafile))
						 {
							 fscanf(datafile, "%d %d %d %d %d %d %d\n", &yeari, &monthi, &dayi, &houri, &mini, &seci, &mseci);
							 if (prev > -1)tt = (float)(mini) * 60 + (float)seci - prev; else tt = -1;
							 prev = (float)(mini) * 60 + (float)seci;
							 fscanf(datafile, "%f %f %f %f %f %f %f %f %f %f %f %f %f\n", &t1i, &t2i, &t3i, &p1i, &p2i, &p3i, &ch1i, &ch2i, &ch3i, &thi, &phi, &thri, &phri);
							 for (int i = 0; i < 200; i++)
								 fscanf(datafile, "%f %f %f\n", &vol1i, &vol2i, &vol3i);
							 if (!(thri < 0 && phri < 0 && thi < 0 && phi < 0))
							 {
								 hv1->Fill(p1i);
								 hv2->Fill(p2i);
								 hv3->Fill(p3i);
								 hv4->Fill(t1i - t3i);
								 hv5->Fill(t2i - t3i);
								 hv6->Fill(t1i - t2i);
								 hv7->Fill(tt / 60);
								 hv8->Fill(thi);
								 hv9->Fill(phi);

							 }
						 }
						 fclose(datafile);
					 }
				 }
			 }
		 }
	 }

	 year = year1; month = month1;
	for (int day = day1; day < 32; day++)	
	{
		for (int hour = 0; hour < 24; hour++)
		{
			char buffer[150];
					 //sprintf(buffer, "D:\\Save_Pulses_Calibration\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
					 sprintf(buffer, "D:\\Save_Pulses_Showers_Rec\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
		    FILE* datafile = fopen(buffer, "r");
					 if (datafile != 0)
					 {
						 cout << "opening file " << buffer << " " << year << " " << month << " " << day << " " << hour << endl; //getchar();
						 float prev = -1;
						 while (!feof(datafile))
						 {
							 fscanf(datafile, "%d %d %d %d %d %d %d\n", &yeari, &monthi, &dayi, &houri, &mini, &seci, &mseci);
							 if (prev > -1)tt = (float)(mini) * 60 + (float)seci - prev; else tt = -1;
							 prev = (float)(mini) * 60 + (float)seci;
							 fscanf(datafile, "%f %f %f %f %f %f %f %f %f %f %f %f %f\n", &t1i, &t2i, &t3i, &p1i, &p2i, &p3i, &ch1i, &ch2i, &ch3i, &thi, &phi, &thri, &phri);
							 for (int i = 0; i < 200; i++)
								 fscanf(datafile, "%f %f %f\n", &vol1i, &vol2i, &vol3i);
							 if (!(thri < 0 && phri < 0 && thi < 0 && phi < 0))
							 {
								 hv1->Fill(p1i);
								 hv2->Fill(p2i);
								 hv3->Fill(p3i);
								 hv4->Fill(t1i - t3i);
								 hv5->Fill(t2i - t3i);
								 hv6->Fill(t1i - t2i);
								 hv7->Fill(tt / 60);
								 hv8->Fill(thi);
								 hv9->Fill(phi);
							 }
						 }
						 fclose(datafile);
					 }
		}
	 }



	year = year2; month = month2;
	for (int day = 1; day < day2+1; day++)
	{
		for (int hour = 0; hour < 24; hour++)
		{
			char buffer[150];
			//sprintf(buffer, "D:\\Save_Pulses_Calibration\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
					 sprintf(buffer, "D:\\Save_Pulses_Showers_Rec\\events_%d_%d_%d_%d_%d", station, year, month, day, hour);
			FILE* datafile = fopen(buffer, "r");
			if (datafile != 0)
			{
				cout << "opening file " << buffer << " " << year << " " << month << " " << day << " " << hour << endl; //getchar();
				float prev = -1;
				while (!feof(datafile))
				{
					fscanf(datafile, "%d %d %d %d %d %d %d\n", &yeari, &monthi, &dayi, &houri, &mini, &seci, &mseci);
					if (prev > -1)tt = (float)(mini) * 60 + (float)seci - prev; else tt = -1;
					prev = (float)(mini) * 60 + (float)seci;
					fscanf(datafile, "%f %f %f %f %f %f %f %f %f %f %f %f %f\n", &t1i, &t2i, &t3i, &p1i, &p2i, &p3i, &ch1i, &ch2i, &ch3i, &thi, &phi, &thri, &phri);
					for (int i = 0; i < 200; i++)
						fscanf(datafile, "%f %f %f\n", &vol1i, &vol2i, &vol3i);
					if (!(thri < 0 && phri < 0 && thi < 0 && phi < 0))
					{
						hv1->Fill(p1i);
						hv2->Fill(p2i);
						hv3->Fill(p3i);
						hv4->Fill(t1i - t3i);
						hv5->Fill(t2i - t3i);
						hv6->Fill(t1i - t2i);
						hv7->Fill(tt / 60);
						hv8->Fill(thi);
						hv9->Fill(phi);
					}
				}
				fclose(datafile);
			}
		}
	}
	*/

 }

gr1->SetTitle("Pulse 1");
gr2->SetTitle("Pulse 2");
gr3->SetTitle("Pulse 3");
gr1->GetXaxis()->SetTitle("ns");
gr2->GetXaxis()->SetTitle("ns");
gr3->GetXaxis()->SetTitle("ns");
gr1->GetYaxis()->SetTitle("mV");
gr2->GetYaxis()->SetTitle("mV");
gr3->GetYaxis()->SetTitle("mV");

gStyle->SetTitleFontSize(0.08);
gStyle->SetStatFontSize(0.08);

//gr1->GetXaxis()->SetRangeUser(600, 1600);
//gr2->GetXaxis()->SetRangeUser(600, 1600);
//gr3->GetXaxis()->SetRangeUser(600, 1600);
//gr1->GetYaxis()->SetTitle("mV");
//gr1->GetXaxis()->SetTitle("ns");
//gr1->SetTitle("Mean Pulse CH1");
//gr2->GetYaxis()->SetTitle("mV");
//gr2->GetXaxis()->SetTitle("ns");
//gr2->SetTitle("Mean Pulse CH2");
//gr3->GetYaxis()->SetTitle("mV");
//gr3->GetXaxis()->SetTitle("ns");
//gr3->SetTitle("Mean Pulse CH3");
//gr1->GetYaxis()->SetRangeUser(0, 20);
//gr2->GetYaxis()->SetRangeUser(0, 20);
//gr3->GetYaxis()->SetRangeUser(0, 20);
   gr1->SetFillColor(kBlue);
   gr1->SetLineColor(kBlue);
   gr1->SetFillStyle(1001);
   gr2->SetFillColor(kGreen);
   gr2->SetLineColor(kGreen);
   gr2->SetFillStyle(1001);
   gr3->SetFillColor(kRed);
   gr3->SetLineColor(kRed);
   gr3->SetFillStyle(1001);
gr1->GetYaxis()->SetLabelSize(0.07);
gr1->GetYaxis()->SetTitleSize(0.07);
gr1->GetXaxis()->SetLabelSize(0.07);
gr1->GetXaxis()->SetTitleSize(0.07);
gr2->GetYaxis()->SetLabelSize(0.07);
gr2->GetYaxis()->SetTitleSize(0.07);
gr2->GetXaxis()->SetLabelSize(0.07);
gr2->GetXaxis()->SetTitleSize(0.07);
gr3->GetYaxis()->SetLabelSize(0.07);
gr3->GetYaxis()->SetTitleSize(0.07);
gr3->GetXaxis()->SetLabelSize(0.07);
gr3->GetXaxis()->SetTitleSize(0.07);

c1->Divide(3,1);

TPad* pad= (TPad*)c1->GetPad(1);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(2);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(3);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
c1->cd(1);
gr1->Draw("AB");
c1->cd(2);
gr2->Draw("AB");
c1->cd(3);
gr3->Draw("AB");
c1->SaveAs("pulses1.jpg");

double rn;
rn=hvv1->GetEntries();
auto c2 = new TCanvas("c2", "Candle Decay", 1198, 974);
c2->Divide(3, 3);
pad = (TPad*)c2->GetPad(1);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(2);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(3);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(4);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(5);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(6);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(7);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(8);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);
pad = (TPad*)c2->GetPad(9);
pad->SetBottomMargin(0.15);
pad->SetLeftMargin(0.2);


hvv1->GetXaxis()->SetTitle("mV");
hvv2->GetXaxis()->SetTitle("mV");
hvv3->GetXaxis()->SetTitle("mV");
hvv1->GetYaxis()->SetTitle("Events");
hvv2->GetYaxis()->SetTitle("Events");
hvv3->GetYaxis()->SetTitle("Events");
hvv4->GetXaxis()->SetTitle("ns");
hvv5->GetXaxis()->SetTitle("ns");
hvv6->GetXaxis()->SetTitle("ns");
hvv4->GetYaxis()->SetTitle("Events");
hvv5->GetYaxis()->SetTitle("Events");
hvv6->GetYaxis()->SetTitle("Events");
hvv7->GetXaxis()->SetTitle("minutes");
hvv8->GetXaxis()->SetTitle("Degrees");
hvv9->GetXaxis()->SetTitle("Degrees");
hvv7->GetYaxis()->SetTitle("Events");
hvv8->GetYaxis()->SetTitle("Events");
hvv9->GetYaxis()->SetTitle("Events");

c2->cd(1);
hvv1->DrawNormalized("E1",rn);
hv1->DrawNormalized("SAME",rn);
//hv2->Draw("S");
c2->cd(2);
hvv2->DrawNormalized("E1",rn);
hv2->DrawNormalized("SAME",rn);
c2->cd(3);
hvv3->DrawNormalized("E1",rn);
hv3->DrawNormalized("SAME",rn);

c2->cd(4);
hvv4->DrawNormalized("E1",rn);
hv4->DrawNormalized("SAME",rn);
c2->cd(5);
hvv5->DrawNormalized("E1",rn);
hv5->DrawNormalized("SAME",rn);
c2->cd(6);
hvv6->DrawNormalized("E1",rn);
hv6->DrawNormalized("SAME",rn);

c2->cd(7);
hvv7->DrawNormalized("E1",rn);
hv7->DrawNormalized("SAME",rn);
c2->cd(8);
hvv8->DrawNormalized("E1",rn);
hv8->DrawNormalized("SAME",rn);
c2->cd(9);
hvv9->GetYaxis()->SetRangeUser(0,2*rn/10.0);
hvv9->DrawNormalized("E1",rn);
hv9->DrawNormalized("SAME",rn);

c2->SaveAs("plots.jpg");

}