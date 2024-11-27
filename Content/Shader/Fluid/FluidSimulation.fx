sampler VelocitySampler : register(s0);

float2 resolution;
float dt;
float viscosity;
float2 forcePosition;
float forceRadius;
float2 forceDirection;
float forceStrength;
float densityDecay;
float2 texelSize;
float diffuseType;
float maxDensity;

float4 SampleVelocity(float2 uv)
{
    return tex2D(VelocitySampler, uv);
}

float4 vL(float2 uv)
{
    return SampleVelocity(uv - float2(texelSize.x, 0));
}

float4 vR(float2 uv)
{
    return SampleVelocity(uv + float2(texelSize.x, 0));
}

float4 vT(float2 uv) {
    return SampleVelocity(uv + float2(0, texelSize.y));
}

float4 vB(float2 uv) {
    return SampleVelocity(uv - float2(0, texelSize.y));
}

float random(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float Curl(float2 uv) {
    float l = vL(uv).y;
    float r = vR(uv).y;
    float t = vT(uv).x;
    float b = vB(uv).x;
    float curl = (r - l) - (t - b);
    return 0.5 * curl;
}

float Divergence(float2 uv, float4 velocityDensity) {
    float l = vL(uv).x;
    float r = vR(uv).x;
    float t = vT(uv).y;
    float b = vB(uv).y;

    if (vL.x < 0.0) { L = -C.x; }
    if (vR.x > 1.0) { R = -C.x; }
    if (vT.y > 1.0) { T = -C.y; }
    if (vB.y < 0.0) { B = -C.y; }

    return 0.5 * (R - L + T - B);
}

float Pressure(float2 uv) {
    float L = texture2D(uPressure, vL).x;
    float R = texture2D(uPressure, vR).x;
    float T = texture2D(uPressure, vT).x;
    float B = texture2D(uPressure, vB).x;
    float C = texture2D(uPressure, vUv).x;
    float divergence = texture2D(uDivergence, vUv).x;
    float pressure = (L + R + B + T - divergence) * 0.25;
    gl_FragColor = vec4(pressure, 0.0, 0.0, 1.0);

}

float4 AddVorticity(float2 uv, float4 velocityDensity)
{
    float curlL = Curl(uv - float2(texelSize.x, 0));
    float curlR = Curl(uv + float2(texelSize.x, 0));
    float curlT = Curl(uv + float2(0, texelSize.y));
    float curlB = Curl(uv - float2(0, texelSize.y));
    float curlC = Curl(uv);

    float2 force = 0.5 * float2(abs(curlT) - abs(curlB), abs(curlR) - abs(curlL));
    force = force / length(force + 0.0001);
    force = force * curlC;
    force.y = -force.y;
    float2 velocity = velocityDensity.rg;
    velocity += force * dt;
    velocity = min(max(velocity, -1000.0), 1000.0);

    return float4(velocity, velocityDensity.b, velocityDensity.a);
}

float4 AddForce(float2 uv, float4 velocityDensity)
{
    float2 velocity = velocityDensity.rg;
    float density = velocityDensity.a;

    float dist = length(uv - forcePosition);
    if (dist < forceRadius)
    {
        float influence = 1.0 - (dist / forceRadius);
        float2 radial = normalize(uv - forcePosition) * forceStrength * influence;
        float2 noise = float2(
            (random(uv * 10.0) - 0.5) * 0.1,
            (random(uv * 20.0) - 0.5) * 0.1
        );

        velocity += radial + noise;
        density += forceStrength * influence * 0.1;
    }

    return float4(velocity, velocityDensity.b, density);
}

float4 AddRandomTurbulence(float2 uv, float4 velocityDensity)
{
    float density = velocityDensity.a;

    // 添加随机扰动
    float turbulence = (random(uv * 50.0) - 0.5) * 0.1;
    density += turbulence;

    return float4(velocityDensity.rg, velocityDensity.b, density);
}

// 平流 (Advection)
float4 Advect(float2 uv, float4 velocityDensity)
{
    float2 velocity = velocityDensity.rg;

    float2 prevPos = uv - velocity * dt / resolution;
    float4 prevVelocityDensity = SampleVelocity(prevPos);
    return prevVelocityDensity;
}

float4 Diffuse(float2 uv, float4 velocityDensity)
{
    float2 velocity = velocityDensity.rg;
    float density = velocityDensity.a;

    // switch diffusion implementations
    if (diffuseType == 0.0) // gaussian blur
    {
        float kernel[9] = {
            1.0 / 16, 2.0 / 16, 1.0 / 16,
            2.0 / 16, 4.0 / 16, 2.0 / 16,
            1.0 / 16, 2.0 / 16, 1.0 / 16
        };
        float2 diffusedVelocity = velocity;
        float diffusedDensity = density;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                float2 samplePos = uv + float2(i, j) / resolution;
                float4 sample = SampleVelocity(samplePos);
                diffusedVelocity += sample.rg * kernel[(i + 1) * 3 + (j + 1)];
                diffusedDensity += sample.a * kernel[(i + 1) * 3 + (j + 1)];
            }
        }

        return float4(
            lerp(velocity, diffusedVelocity, viscosity),
            velocityDensity.b,
            lerp(density, diffusedDensity, viscosity)
        );
    }
    else if (diffuseType == 1.0)
    {
        float laplacianDensity =
              SampleVelocity(uv + float2(1, 0) / resolution).a
            + SampleVelocity(uv + float2(-1, 0) / resolution).a
            + SampleVelocity(uv + float2(0, 1) / resolution).a
            + SampleVelocity(uv + float2(0, -1) / resolution).a
            - 4.0 * density;

        float2 laplacianVelocity =
              SampleVelocity(uv + float2(1, 0) / resolution).rg
            + SampleVelocity(uv + float2(-1, 0) / resolution).rg
            + SampleVelocity(uv + float2(0, 1) / resolution).rg
            + SampleVelocity(uv + float2(0, -1) / resolution).rg
            - 4.0 * velocity;

        return float4(
            velocity + viscosity * laplacianVelocity,
            velocityDensity.b,
            density + viscosity * laplacianDensity
        );
    }
    else if (diffuseType == 2.0) // random diffusion
    {
        float randomNoise = random(uv * 100.0) * 0.1 - 0.05;
        return float4(
            velocity + float2(randomNoise, randomNoise) * viscosity,
            velocityDensity.b,
            density + randomNoise * viscosity
        );
    }
    else if (diffuseType == 3.0) // wind driven diffusion
    {
        float2 wind = float2(0.1, 0.2); // static wind
        float4 windSample = SampleVelocity(uv - wind * dt / resolution);
        return float4(
            lerp(velocity, windSample.rg, viscosity),
            velocityDensity.b,
            lerp(density, windSample.a, viscosity)
        );
    }
    else
    {
        return velocityDensity;
    }
}

float4 FluidSimulate(float2 uvInput : TEXCOORD0) : COLOR0
{
    float2 uv = uvInput;
    // if (texelSize > 0.0) {
    //     uv = floor(uv / texelSize) * texelSize;
    // }
    float4 velocityDensity = SampleVelocity(uv);

    velocityDensity = AddForce(uv, velocityDensity);
    velocityDensity = AddVorticity(uv, velocityDensity);
    velocityDensity = AddRandomTurbulence(uv, velocityDensity);
    velocityDensity = Advect(uv, velocityDensity);
    velocityDensity = Diffuse(uv, velocityDensity);
    velocityDensity.a *= exp(-densityDecay * dt);
    velocityDensity.a = min(velocityDensity.a, maxDensity);

    return velocityDensity;
}

technique FluidSimulation
{
    pass P0
    {
        PixelShader = compile ps_3_0 FluidSimulate();
    }
}