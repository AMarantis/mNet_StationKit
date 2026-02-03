void timing()
{
	auto c1 = new TCanvas("c1","Candle Decay",1500,600);
	c1->SetFillColor(204);
	gStyle->SetStatStyle(204);
auto	hv1= new  TH1F("Stats", "T3-T1", 50, -40.0 , 40.0);  // Create a 1D histogram object of floats
auto	hv2= new  TH1I("Stats", "T3-T2", 50, -40.0 , 40.0);  // Create a 1D histogram object of floats
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
   


float vvt1,vvt2;
float vvt3,vvt4;
 int ic1=0;
 FILE* inp1=fopen("timing.txt","r");
 if(inp1!=0)
 {
 while(! feof(inp1))
 {
	fscanf(inp1,"%g %g \n",&vvt1, &vvt2);
	//if(vvt3>-999) hv1->Fill(float(vvt3)*1.0);
	//if(vvt4>-999) hv2->Fill(float(vvt4)*1.0);
	if(vvt1>-999) hv1->Fill(float(vvt1)*1.0);
	if(vvt2>-999) hv2->Fill(float(vvt2)*1.0);
	ic1++;
 }
 cout<<ic1<<endl;fclose(inp1);
 }
 cout<<"first part completed"<<endl;

gStyle->SetOptFit(2);
gStyle->SetTitleFontSize(0.08);
gStyle->SetStatFontSize(0.03);

float size=0.05;
hv1->GetYaxis()->SetLabelSize(size);
hv1->GetYaxis()->SetTitleSize(size);
hv1->GetXaxis()->SetLabelSize(size);
hv1->GetXaxis()->SetTitleSize(size);
hv2->GetYaxis()->SetLabelSize(size);
hv2->GetYaxis()->SetTitleSize(size);
hv2->GetXaxis()->SetLabelSize(size);
hv2->GetXaxis()->SetTitleSize(size);

hv1->GetXaxis()->SetTitle("ns");
hv1->GetYaxis()->SetTitle("Entries");
hv2->GetXaxis()->SetTitle("ns");
hv2->GetYaxis()->SetTitle("Entries");

//hv2->Fit("gaus");
c1->Divide(2,1);

TPad* pad= (TPad*)c1->GetPad(1);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);
 pad= (TPad*)c1->GetPad(2);
 pad->SetBottomMargin(0.15);
 pad->SetLeftMargin(0.2);

c1->cd(1);
hv1->Fit("gaus");
//hv1->Draw();
c1->cd(2);
hv2->Fit("gaus");
//hv2->Draw();
c1->SaveAs("outroot2.jpg");

}