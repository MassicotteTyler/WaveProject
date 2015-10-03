#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <time.h>

#define N 16
#define PI2 6.2832

float* DFT();

extern "C"
{
	__declspec(dllexport) void DFTDLL()
	{
		DFT();
	}
}

float* DFT()
{
	// time and frequency domain data arrays
	int n, k;             // indices for time and frequency domains
	float x[N];           // discrete-time signal, x
	float Xre[N], Xim[N]; // DFT of x (real and imaginary parts)
	float P[N];           // power spectrum of x

	// Generate random discrete-time signal x in range (-1,+1)
	srand(time(0));
	for (n = 0; n<N; ++n) x[n] = ((2.0 * rand()) / RAND_MAX) - 1.0;

	// Calculate DFT of x using brute force
	for (k = 0; k<N; ++k)
	{
		// Real part of X[k]
		Xre[k] = 0;
		for (n = 0; n<N; ++n) Xre[k] += x[n] * cos(n * k * PI2 / N);

		// Imaginary part of X[k]
		Xim[k] = 0;
		for (n = 0; n<N; ++n) Xim[k] -= x[n] * sin(n * k * PI2 / N);

		// Power at kth frequency bin
		P[k] = Xre[k] * Xre[k] + Xim[k] * Xim[k];
	}

	return Xre;
	
	

}