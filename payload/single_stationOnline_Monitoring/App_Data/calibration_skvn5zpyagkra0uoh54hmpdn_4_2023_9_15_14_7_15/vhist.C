void vhist()
{
	auto c1 = new TCanvas("c1","Candle Decay",1500,600);
	c1->SetFillColor(204);
	gStyle->SetStatStyle(204);
auto	hv1= new  TH1F("Stats", "Pulse Height 1", 20, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv1t1= new  TH1F("Stats", "Pulse Height CH1 trigger 1", 200, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv1t2= new  TH1F("Stats", "Pulse Height CH1 trigger 2", 200, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv2= new  TH1I("Stats", "Pulse Height 2", 20, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv2t1= new  TH1F("Stats", "Pulse Height CH2 trigger 1", 200, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv2t2= new  TH1F("Stats", "Pulse Height CH2 trigger 2", 200, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv3= new  TH1I("Stats", "Pulse Height 3", 20, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv3t1= new  TH1F("Stats", "Pulse Height CH3 trigger 1", 200, -5.0 , 55.0);  // Create a 1D histogram object of floats
auto	hv3t2= new  TH1F("Stats", "Pulse Height CH3 trigger 2", 200, -5.0 , 55.0);  // Create a 1D histogram object of floats
   hv1->SetBarWidth(3);
   hv1->SetFillStyle(0);
//   hv1->GetYaxis()->SetTitle("Entries");
//   hv1->GetXaxis()->SetTitle("mV");
   hv1->SetFillColor(kBlue);
   hv1->SetLineColor(kBlue);
   hv1->SetFillStyle(1001);

   hv2->SetBarWidth(3);
   hv2->SetFillStyle(0);
   hv2->SetFillColor(kGray);
//   hv2->GetYaxis()->SetTitle("Entries");
//   hv2->GetXaxis()->SetTitle("mV");
   hv2->SetFillColor(kGreen);
   hv2->SetLineColor(kGreen);
   hv2->SetFillStyle(1001);
   
   hv3->SetBarWidth(3);
   hv3->SetFillStyle(0);
   hv3->SetFillColor(kGray);
 //  hv3->GetYaxis()->SetTitle("Entries");
 //  hv3->GetXaxis()->SetTitle("mV");
   hv3->SetFillColor(kRed);
   hv3->SetLineColor(kRed);
   hv3->SetFillStyle(1001);


 float vv, vvt1,vvt2, avg1,avg2,avg3;
 int ic1=0;
 int ic2=0;
 int ic3=0;
 FILE* inp1=fopen("test1.txt","r");
 if(inp1!=0)
 {
 while(! feof(inp1))
 {
	fscanf(inp1,"%g\n",&vv);
	if(vv>3)hv1->Fill(vv);
	hv1t1->Fill(vvt1);
	hv1t2->Fill(vvt2);
	ic1++;
 }
 cout<<ic1<<endl;fclose(inp1);
 }
 FILE* inp2=fopen("test2.txt","r");
 if(inp2!=0)
 {
 while(! feof(inp2))
 {
	fscanf(inp2,"%g\n",&vv);
	if(vv>3)hv2->Fill(vv);
	hv2t1->Fill(vvt1);
	hv2t2->Fill(vvt2);
	ic2++;
 }
 cout<<ic2<<endl;fclose(inp2);
 }
 FILE* inp3=fopen("test3.txt","r");
 if(inp3!=0)
 {
 while(! feof(inp3))
 {
	fscanf(inp3,"%g\n",&vv);
	if(vv>3)hv3->Fill(vv);
	hv3t1->Fill(vvt1);
	hv3t2->Fill(vvt2);
	ic3++;
 }
 cout<<ic3<<endl;fclose(inp3);
 }
 cout<<"first part completed"<<endl;
 float vol,time,v0,v1,v2;int i=0;
 float pulse1[4096];
 float pulse2[4096];
 float pulse3[4096];
 float x1[4096],y1[4096];
 if(inp1!=0) fclose(inp1);
 inp1=fopen("pulse1.txt","r");
 float amax1=0.0;
 if(inp1!=0)
 {
 while(! feof(inp1))
 {
	fscanf(inp1,"%f\n",&vol);
     y1[i]=vol;
	 x1[i]=float(i)*4.0;
	 if(vol>amax1)amax1=vol;
	 i++;
 }
 fclose(inp1);
 }
 cout<<i<<endl;
TGraph* gr1= new TGraph(i,x1,y1);

  i=0;
 float x2[4096],y2[4096];
  if(inp2!=0)fclose(inp2);
  inp2=fopen("pulse2.txt","r");
 float amax2=0.0;
 if(inp2!=0)
 {
 while(! feof(inp2))
 {
	fscanf(inp2,"%f\n",&vol);
     y2[i]=vol;
	 x2[i]=float(i)*4.0;
	 if(vol>amax2)amax2=vol;
	 i++;
 }
 fclose(inp2);
 }
 cout<<i<<endl;
TGraph* gr2= new TGraph(i,x2,y2);

  i=0;
 float x3[4096],y3[4096];
 if(inp3!=0) fclose(inp3);
 inp3=fopen("pulse3.txt","r");
 float amax3=0.0;
 if(inp3!=0)
 {
 while(! feof(inp3))
 {
	fscanf(inp3,"%f\n",&vol);
     y3[i]=vol;
	 if(vol>amax3)amax3=vol;
	 x3[i]=float(i)*4.0;
	 i++;
 }
 fclose(inp3);
 }
 cout<<i<<endl;
 cout<<"second part completed OK"<<endl;
 TGraph* gr3= new TGraph(i,x3,y3);

gr1->SetTitle("Mean Pulse 1");
gr2->SetTitle("Mean Pulse 2");
gr3->SetTitle("Mean Pulse 3");
gr1->GetXaxis()->SetTitle("ns");
gr2->GetXaxis()->SetTitle("ns");
gr3->GetXaxis()->SetTitle("ns");
gr1->GetYaxis()->SetTitle("mV");
gr2->GetYaxis()->SetTitle("mV");
gr3->GetYaxis()->SetTitle("mV");

gStyle->SetTitleFontSize(0.08);
gStyle->SetStatFontSize(0.08);

gr1->GetXaxis()->SetRangeUser(600, 1600);
gr2->GetXaxis()->SetRangeUser(600, 1600);
gr3->GetXaxis()->SetRangeUser(600, 1600);
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

hv1->GetYaxis()->SetLabelSize(0.07);
hv1->GetYaxis()->SetTitleSize(0.07);
hv1->GetXaxis()->SetLabelSize(0.07);
hv1->GetXaxis()->SetTitleSize(0.07);
hv2->GetYaxis()->SetLabelSize(0.07);
hv2->GetYaxis()->SetTitleSize(0.07);
hv2->GetXaxis()->SetLabelSize(0.07);
hv2->GetXaxis()->SetTitleSize(0.07);
hv3->GetYaxis()->SetLabelSize(0.07);
hv3->GetYaxis()->SetTitleSize(0.07);
hv3->GetXaxis()->SetLabelSize(0.07);
hv3->GetXaxis()->SetTitleSize(0.07);

hv1->GetXaxis()->SetTitle("mV");
hv1->GetYaxis()->SetTitle("Entries");
hv2->GetXaxis()->SetTitle("mV");
hv2->GetYaxis()->SetTitle("Entries");
hv3->GetXaxis()->SetTitle("mV");
hv3->GetYaxis()->SetTitle("Entries");
c1->Divide(3,2);

TPad* pad= (TPad*)c1->GetPad(1);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(2);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(3);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(4);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(5);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(6);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);

c1->cd(4);
hv1->Draw();
c1->cd(5);
hv2->Draw();
c1->cd(6);
hv3->Draw();
c1->cd(1);
gr1->Draw("AB");
c1->cd(2);
gr2->Draw("AB");
c1->cd(3);
gr3->Draw("AB");
c1->SaveAs("outroot.jpg");
/*
	auto c2 = new TCanvas("c1","Candle Decay",1000,300);
i=0;float max;
 fclose(inp1);inp3=fopen("pulses.txt","r");
 while(! feof(inp1))
 {
	 
	if(i<4096)
	{
		fscanf(inp1,"%f %f %f\n",&v0,&v1,&v2);
	    x3[i]=float(i)-750;
	    pulse1[i]=v0;
	    pulse2[i]=v1;
	    pulse3[i]=v2;
	}else fscanf(inp1,%f\n",&max);
	 i++;
 }
 cout<<i<<endl;

 pad= (TPad*)c2->GetPad(0);
 pad->SetBottomMargin(0.15);

 TGraph* grv1= new TGraph(i,x3,pulse1);
 TGraph* grv2= new TGraph(i,x3,pulse2);
 TGraph* grv3= new TGraph(i,x3,pulse3);
   grv1->SetFillColor(kRed);
   grv1->SetLineColor(kRed);
   grv1->SetFillStyle(1001);
   grv2->SetFillColor(kBlue);
   grv2->SetLineColor(kBlue);
   grv2->SetFillStyle(1001);
   grv3->SetFillColor(kGreen);
   grv3->SetLineColor(kGreen);
   grv3->SetFillStyle(1001);
grv1->GetYaxis()->SetTitleSize(0.07);
grv1->GetXaxis()->SetTitleSize(0.07);
grv1->GetYaxis()->SetTitle("mV");
grv1->GetXaxis()->SetTitle("ns");
grv1->GetYaxis()->SetLabelSize(0.07);
grv1->GetXaxis()->SetLabelSize(0.07);
grv1->GetYaxis()->SetRangeUser(0., max+2.0);
grv1->GetXaxis()->SetRangeUser(0., 750.);
grv1->Draw();
grv2->Draw("SAME");
grv3->Draw("SAME");
if(max>5)c2->SaveAs("pulses.png");
*/

}