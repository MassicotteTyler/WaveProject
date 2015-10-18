#include <Windows.h>
#include <mmsystem.h>
#include <stdlib.h>
#include <stdio.h>

#define BUFFER_SIZE 16384
short* Record();
short int waveIn[11025 * 10]; //10 seconds

extern "C"
{
	__declspec(dllexport) void RECORDDLL()
	{
		Record();
	}
}


short* Record()
{
	const int sampleRate = 11025;
	int num_inputs = sampleRate * 10; // 10 seconds

	HWAVEIN hWaveIn;
	WAVEHDR WaveInHdr;
	MMRESULT result;

	//Recording Parameters 
	WAVEFORMATEX pFormat;
	pFormat.wFormatTag = WAVE_FORMAT_PCM;
	pFormat.nChannels = 1;
	pFormat.nSamplesPerSec = sampleRate;
	pFormat.nAvgBytesPerSec = sampleRate;
	pFormat.nBlockAlign = 1;
	pFormat.wBitsPerSample = 8;
	pFormat.cbSize = 0;

	result = waveInOpen(&hWaveIn, WAVE_MAPPER, &pFormat, 0, 0, WAVE_FORMAT_DIRECT);

	if (result)
	{
		//Error
		return 0;
	}

	//Setup and prepare header for input
	WaveInHdr.lpData = (LPSTR)waveIn;
	WaveInHdr.dwBufferLength = sampleRate * 10 * 2;
	WaveInHdr.dwBytesRecorded = 0;
	WaveInHdr.dwUser = 0;
	WaveInHdr.dwFlags = 0;
	WaveInHdr.dwLoops = 0;

	waveInPrepareHeader(hWaveIn, &WaveInHdr, sizeof(WAVEHDR));

	//input a wave input buffer
	result = waveInAddBuffer(hWaveIn, &WaveInHdr, sizeof(WAVEHDR));
	if (result)
	{
		//Error
		return 0;
	}

	//Start sampling input
	result = waveInStart(hWaveIn);
	if (result)
	{
		//Error 
		return 0;
	}

	while (waveInUnprepareHeader(hWaveIn, &WaveInHdr, sizeof(WAVEHDR)) == WAVERR_STILLPLAYING);

	waveInClose(hWaveIn);

	return waveIn;
	

}