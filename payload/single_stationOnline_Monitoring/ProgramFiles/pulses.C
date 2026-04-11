void pulses(int iStation, float thres1, float thres2, float thres3)
{
	(void)iStation;

	auto hvv1 = new TH1F("Stats", "Peak Voltage 1", 15, 0.0, 90.0);
	auto hvv2 = new TH1F("Stats", "Peak Voltage 2", 15, 0.0, 90.0);
	auto hvv3 = new TH1F("Stats", "Peak Voltage 3", 15, 0.0, 90.0);
	auto hvv4 = new TH1F("Stats", "T1-T3", 20, -50.0, 50.0);
	auto hvv5 = new TH1F("Stats", "T2-T3", 20, -50.0, 50.0);
	auto hvv6 = new TH1F("Stats", "T1-T2", 20, -50.0, 50.0);
	auto hvv7 = new TH1F("Stats", "DT", 20, 0.0, 60.0);
	auto hvv8 = new TH1F("Stats", "cos(Zenith)", 20, 0.0, 1.0);
	auto hvv9 = new TH1F("Stats", "Azimuth angle", 10, 0.0, 360.0);

	auto c1 = new TCanvas("c1", "Candle Decay", 1500, 300);
	c1->SetFillColor(204);
	gStyle->SetStatStyle(204);

	double vol1 = 0.0, vol2 = 0.0, vol3 = 0.0;
	int ii = 0;
	double pulse1[200];
	double pulse2[200];
	double pulse3[200];
	double x[200];

	FILE* inp1 = fopen("events.txt", "r");
	if (inp1 == 0) return;

	double amax1 = 0.0;
	double amax2 = 0.0;
	double amax3 = 0.0;

	int year, month, day, hour, min, sec, msec;
	double t1, t2, t3, p1, p2, p3, ch1, ch2, ch3, th, ph, thr, phr;
	double prevv = -1;
	double tt = -1;

	while (!feof(inp1))
	{
		if (fscanf(inp1, "%d %d %d %d %d %d %d\n", &year, &month, &day, &hour, &min, &sec, &msec) != 7) break;
		if (fscanf(inp1, "%lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf %lf\n", &t1, &t2, &t3, &p1, &p2, &p3, &ch1, &ch2, &ch3, &th, &ph, &thr, &phr) != 13) break;

		if (prevv > -1) tt = (double)min * 60.0 + (double)sec - prevv; else tt = -1;
		prevv = (double)min * 60.0 + (double)sec;

		if (th > 0 && ph > 0 && p1 > thres1 && p2 > thres2 && p3 > thres3)
		{
			if (tt / 60.0 != 0.0)
			{
				hvv1->Fill(p1);
				hvv2->Fill(p2);
				hvv3->Fill(p3);
				hvv4->Fill(t1 - t3);
				hvv5->Fill(t2 - t3);
				hvv6->Fill(t1 - t2);
				hvv7->Fill(tt / 60.0);
				hvv8->Fill(cos(th * 3.14159 / 180.0));
				hvv9->Fill(ph);
			}
		}

		for (int i = 0; i < 200; i++)
		{
			if (fscanf(inp1, "%lf %lf %lf\n", &vol1, &vol2, &vol3) != 3) break;
			pulse1[i] = vol1;
			pulse2[i] = vol2;
			pulse3[i] = vol3;
			x[i] = (double)i * 4.0;
			if (vol1 > amax1) amax1 = vol1;
			if (vol2 > amax2) amax2 = vol2;
			if (vol3 > amax3) amax3 = vol3;
			ii++;
		}
	}
	fclose(inp1);

	TGraph* gr1 = new TGraph(200, x, pulse1);
	TGraph* gr2 = new TGraph(200, x, pulse2);
	TGraph* gr3 = new TGraph(200, x, pulse3);

	auto c2 = new TCanvas("c2", "Timing plots", 1500, 500);
	c2->Divide(3, 2);
	c2->cd(1); hvv1->Draw();
	c2->cd(2); hvv2->Draw();
	c2->cd(3); hvv3->Draw();
	c2->cd(4); hvv4->Draw();
	c2->cd(5); hvv5->Draw();
	c2->cd(6); hvv6->Draw();
	c2->SaveAs("plots.jpg");

	auto c3 = new TCanvas("c3", "Pulses", 1500, 500);
	c3->Divide(3, 1);
	c3->cd(1); gr1->Draw("AL");
	c3->cd(2); gr2->Draw("AL");
	c3->cd(3); gr3->Draw("AL");
	c3->SaveAs("pulses1.jpg");
}
