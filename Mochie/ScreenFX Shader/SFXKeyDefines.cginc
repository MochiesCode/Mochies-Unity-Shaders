#ifndef SFX_KEYWORD_DEFINES
#define SFX_KEYWORD_DEFINES

#ifndef X_FEATURES
	#define X_FEATURES defined(SFXX)
#endif

#ifndef MAIN_PASS
	#define MAIN_PASS defined(MAIN)
#endif

#ifndef TRIPLANAR_PASS
	#define TRIPLANAR_PASS defined(TRIPLANAR)
#endif

#ifndef ZOOM_PASS
	#define ZOOM_PASS defined(ZOOM)
#endif

#ifndef COLOR_ENABLED
	#define COLOR_ENABLED defined(_COLORCOLOR_ON)
#endif

#ifndef SHAKE_ENABLED
	#define SHAKE_ENABLED defined(FXAA)
#endif

#ifndef DISTORTION_ENABLED
	#define DISTORTION_ENABLED defined(EFFECT_BUMP)
#endif

#ifndef DISTORTION_WORLD_ENABLED
	#define DISTORTION_WORLD_ENABLED defined(_TERRAIN_NORMAL_MAP)
#endif

#ifndef BLUR_PIXEL_ENABLED
	#define BLUR_PIXEL_ENABLED defined(BLOOM)
#endif

#ifndef BLUR_DITHER_ENABLED
	#define BLUR_DITHER_ENABLED defined(GRAIN)
#endif

#ifndef BLUR_RADIAL_ENABLED
	#define BLUR_RADIAL_ENABLED defined(_SUNDISK_SIMPLE)
#endif

#ifndef BLUR_Y_ENABLED
	#define BLUR_Y_ENABLED defined(BLOOM_LENS_DIRT)
#endif

#ifndef BLUR_ENABLED
	#define BLUR_ENABLED defined(BLOOM) || defined(GRAIN) || defined(_SUNDISK_SIMPLE)
#endif

#ifndef CHROM_ABB_ENABLED
	#define CHROM_ABB_ENABLED defined(CHROMATIC_ABBERATION_LOW)
#endif

#ifndef DOF_ENABLED
	#define DOF_ENABLED defined(DEPTH_OF_FIELD)
#endif

#ifndef ZOOM_ENABLED
	#define ZOOM_ENABLED defined(_DETAIL_MULX2)
#endif

#ifndef ZOOM_RGB_ENABLED
	#define ZOOM_RGB_ENABLED defined(_MAPPING_6_FRAMES_LAYOUT)
#endif

#ifndef IMAGE_OVERLAY_ENABLED
	#define IMAGE_OVERLAY_ENABLED defined(_COLOROVERLAY_ON)
#endif

#ifndef IMAGE_OVERLAY_DISTORTION_ENABLED
	#define IMAGE_OVERLAY_DISTORTION_ENABLED defined(_PARALLAXMAP)
#endif

#ifndef FOG_ENABLED
	#define FOG_ENABLED defined(_FADING_ON)
#endif

#ifndef TRIPLANAR_ENABLED
	#define TRIPLANAR_ENABLED defined(PIXELSNAP_ON)
#endif

#ifndef OUTLINE_ENABLED
	#define OUTLINE_ENABLED defined(_COLORADDSUBDIFF_ON)
#endif

#ifndef NOISE_ENABLED
	#define NOISE_ENABLED defined(_REQUIRE_UV2)
#endif

#endif