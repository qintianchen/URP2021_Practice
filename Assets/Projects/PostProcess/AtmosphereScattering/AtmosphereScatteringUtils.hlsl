#ifndef __H_ATMOSPHERESCATTERING__
#define __H_ATMOSPHERESCATTERING__

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"

// 假设人物永远处于地表
struct AtmosphereParams
{
    float planetRadius;
    float atmosphereHeight;
    float3 rayleighScattering_h0;
    float rayleighHeight;
    float3 mieScattering_h0;
    float mieHeight;
    float mieAnisotropy;
    float3 mieAbsorption;
    float3 ozoneAbsorption;
    float ozoneCenter;
    float ozoneWidth;
};

StructuredBuffer<AtmosphereParams> _AtmosphereParamses;

float3 GetRayleightSigma(AtmosphereParams params, float height)
{
    float attenuation = exp(-height / params.rayleighHeight);
    return params.rayleighScattering_h0 * attenuation;
}

float3 GetRayleighScattering(AtmosphereParams params, float cos_theta, float height)
{
    float phase = (3.0 * (1.0 + cos_theta * cos_theta)) / (16.0 * PI);
    return phase * GetRayleightSigma(params, height);
}

float3 GetMieSigma(AtmosphereParams params, float height)
{
    float attenuation = exp(- height / params.mieHeight);
    return params.mieScattering_h0 * attenuation;
}

float3 GetMieScattering(AtmosphereParams params, float cos_theta, float height)
{
    float g = params.mieAnisotropy;
    float g2 = g * g;
    float a = 3.0 * (1 - g2) * (1 + cos_theta * cos_theta);
    float b = 8 * PI * (2 + g2) * pow(1 + g2 - 2 * g * cos_theta, 1.5);
    float phase = a / b;
    return phase * GetMieSigma(params, height);
}

float3 GetMieAbsorption(AtmosphereParams params, float height)
{
    float attenuation = exp(- height / params.mieHeight);
    return params.mieAbsorption * attenuation;
}

float3 GetOzoneAbsorption(AtmosphereParams params, float height)
{
    float attenuation = max(0, 1 - abs(height - params.ozoneCenter) / params.ozoneWidth);
    return params.ozoneAbsorption * attenuation;
}

int GetIntersectPointsWithSphere(float3 fromPosition, float3 ray, float3 spherePosition, float sphereRadius, out float distance1, out float distance2)
{
    float i = ray.x;
    float j = ray.y;
    float k = ray.z;

    float a = spherePosition.x;
    float b = spherePosition.y;
    float c = spherePosition.z;

    float m = fromPosition.x;
    float n = fromPosition.y;
    float l = fromPosition.z;

    float r = sphereRadius;

    float ta = i * i + j * j + k * k;
    float tb = 2 * (i * (m - a) + j * (n - b) + k * (l - c));
    float tc = (m - a) * (m - a) + (n - b) * (n - b) + (l - c) * (l - c) - r * r;

    float rp = tb * tb - 4 * ta * tc;

    if (rp < 0)
    {
        return 0;
    }

    if (rp == 0)
    {
        distance1 = -tb / (2 * ta);
        distance2 = -tb / (2 * ta);
        return 1;
    }

    float t1 = (-tb + sqrt(rp)) / (2 * ta);
    float t2 = (-tb - sqrt(rp)) / (2 * ta);

    if (t1 < 0 && t2 < 0)
    {
        return 0;
    }

    if (t1 > 0 && t2 > 0)
    {
        distance1 = min(t1, t2);
        distance2 = max(t1, t2);
        return 2;
    }

    distance1 = max(t1, t2);
    return 1;
}

float TriangleCosineLaw(float a, float b, float cos_theta)
{
    float a2 = a * a;
    float b2 = b * b;
    return sqrt(a2 + b2 - 2 * a * b * cos_theta);
}

float3 GetTransmittance(AtmosphereParams params, float height, float cos_theta)
{
    float sin_theta = sqrt(1 - cos_theta * cos_theta);
    float3 viewDirWS = float3(sin_theta, cos_theta, 0);
    float3 viewPositionWS = float3(0, height, 0);

    float sampleCount = 32;
    float distance1;
    float tempFloat;
    GetIntersectPointsWithSphere(viewPositionWS, viewDirWS, float3(0, -params.planetRadius, 0), params.planetRadius + params.atmosphereHeight, distance1, tempFloat);
    float stepLength = distance1 / sampleCount;
    float3 transmittanceSigma = 0;
    for (int i = 0; i < sampleCount; i++)
    {
        float c = TriangleCosineLaw(params.planetRadius + height, stepLength * (i + 0.5), -cos_theta);
        float curHeight = c - params.planetRadius;
        transmittanceSigma += GetRayleightSigma(params, curHeight) + GetMieSigma(params, curHeight) + GetMieAbsorption(params, curHeight) + GetOzoneAbsorption(params, curHeight);
    }

    return exp(-transmittanceSigma * stepLength);
}

float GetTransmittanceFromLut(AtmosphereParams params, float height, float cos_theta, sampler2D transmittanceLut)
{
    float r = params.planetRadius * params.planetRadius;
    float u = (cos_theta + 1) * 0.5;
    float v = sqrt((height + r) * (height + r) - r * r);
    return tex2D(transmittanceLut, float2(u, v));
}

#endif
