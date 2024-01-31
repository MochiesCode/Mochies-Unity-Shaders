#ifndef AUDIOLINK_BAREBONES_INCLUDED
#define AUDIOLINK_BAREBONES_INCLUDED

uniform float4 _AudioTexture_TexelSize;
SamplerState sampler_AudioTexture;
uniform Texture2D<float4> _AudioTexture;

struct audioLinkData {
	bool textureExists;
	float bass;
	float lowMid;
	float upperMid;
	float treble;
};

float GetAudioLinkBand(audioLinkData al, int band){
	float4 bands = float4(al.bass, al.lowMid, al.upperMid, al.treble);
	return bands[band];
}

void GrabExists(inout audioLinkData al, inout float versionBand){
	float width = 0;
	float height = 0;
	_AudioTexture.GetDimensions(width, height);
	if (width > 64){
		versionBand = 0.0625;
	}
	al.textureExists = width > 16;
}

float SampleAudioTexture(float band){
	return _AudioTexture.SampleLevel(sampler_AudioTexture, float2(0,band),0);
}

void InitializeAudioLink(inout audioLinkData al){
	float versionBand = 1;
	GrabExists(al, versionBand);
	if (al.textureExists){
		al.bass = SampleAudioTexture(0.125 * versionBand);
		al.lowMid = SampleAudioTexture(0.375 * versionBand);
		al.upperMid = SampleAudioTexture(0.625 * versionBand);
		al.treble = SampleAudioTexture(0.875 * versionBand);
	}
}

#endif