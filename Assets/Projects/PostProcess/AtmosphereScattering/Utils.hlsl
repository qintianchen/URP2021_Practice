#pragma once

struct AtmosphereScatteringParams
{
    float planetRadius;
    float atmosphereHeight;

    float ozoneCenter; // 臭氧层中心的海拔高度
    float ozoneHeight; // 臭氧层的厚度

    float3 rayleigh_scattering_sigma;
    float3 mie_scattering_sigma;
    float3 mie_absorption_sigma;
    float3 ozone_absorption_sigma;
};