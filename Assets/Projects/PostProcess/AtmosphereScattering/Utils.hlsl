#pragma once

struct AtmosphereScatteringParams
{
    float planetRadius;
    float atmosphereHeight;

    float ozoneCenter; // 臭氧层中心的海拔高度
    float ozoneHeight; // 臭氧层的厚度

    float3 rayleigh_scattering_h0; // 瑞利散射系数（地表）
    float3 mie_scattering_h0; // 米氏散射系数（地表）
    float3 mie_absorption_h0; // 米氏吸收系数（地表）
    float3 ozone_absorption_h0; // 臭氧层吸收系数（地表）
};

StructuredBuffer<AtmosphereScatteringParams> _AtmosphereScatteringParamses;