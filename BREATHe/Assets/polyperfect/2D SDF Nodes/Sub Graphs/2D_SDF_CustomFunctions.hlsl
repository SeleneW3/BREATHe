//https://iquilezles.org/articles/distfunctions2d/

//Single

float dot2(in float2 v) { return dot(v, v); }
float dot2(in float3 v) { return dot(v, v); }
float ndot(in float2 a, in float2 b) { return a.x * b.x - a.y * b.y; }


//Circle - exact(https://www.shadertoy.com/view/3ltSW2)

void Circle_float(float2 p, float r, out float Out)
{
    Out = length(p) - r;
}

//Rounded Box - exact(https://www.shadertoy.com/view/4llXD7 and https://www.youtube.com/watch?v=s5NGeUV2EyU)

void RoundedBox_float(in float2 p, in float2 b, in float4 r, out float Out)
{
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    float2 q = abs(p) - b + r.x;
    Out = min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

//Box - exact(https://www.youtube.com/watch?v=62-pRVZuS5c)

void Box_float(in float2 p, in float2 b, out float Out)
{
    float2 d = abs(p) - b;
    Out = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

////Segment - exact(https://www.shadertoy.com/view/3tdSDj and https://www.youtube.com/watch?v=PMltMdi1Wzg)

void Segment_float(in float2 p, in float2 a, in float2 b, out float Out)
{
    float2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    Out = length(pa - ba * h);
}

////Rhombus - exact(https://www.shadertoy.com/view/XdXcRB)

void Rhombus_float(in float2 p, in float2 b, out float Out)
{
    p = abs(p);
    float h = clamp(ndot(b - 2.0 * p, b) / dot(b, b), -1.0, 1.0);
    float d = length(p - 0.5 * b * float2(1.0 - h, 1.0 + h));
    Out = d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
}

////Isosceles Trapezoid - exact(https://www.shadertoy.com/view/MlycD3)

void Trapezoid_float (in float2 p, in float r1, float r2, float he, out float Out)
{
    float2 k1 = float2(r2, he);
    float2 k2 = float2(r2 - r1, 2.0 * he);
    p.x = abs(p.x);
    float2 ca = float2(p.x - min(p.x, (p.y < 0.0) ? r1 : r2), abs(p.y) - he);
    float2 cb = p - k1 + k2 * clamp(dot(k1 - p, k2) / dot2(k2), 0.0, 1.0);
    float s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    Out = s * sqrt(min(dot2(ca), dot2(cb)));
}

////Parallelogram - exact(https://www.shadertoy.com/view/7dlGRf)

void Parallelogram_float(in float2 p, float wi, float he, float sk, out float Out)
{
    float2 e = float2(sk, he);
    p = (p.y < 0.0) ? -p : p;
    float2  w = p - e; w.x -= clamp(w.x, -wi, wi);
    float2  d = float2(dot(w, w), -w.y);
    float s = p.x * e.y - p.y * e.x;
    p = (s < 0.0) ? -p : p;
    float2  v = p - float2(wi, 0); v -= e * clamp(dot(v, e) / dot(e, e), -1.0, 1.0);
    d = min(d, float2(dot(v, v), wi * he - abs(s)));
    Out = sqrt(d.x) * sign(-d.y);
}

//Equilateral Triangle - exact(https://www.shadertoy.com/view/Xl2yDW)

void EquilateralTriangle_float(in float2 p, in float scale, out float Out)
{
    p /= scale;
    const float k = sqrt(3.0);
    p.x = abs(p.x) - 1.0;
    p.y = p.y + 1.0 / k;
    if (p.x + k * p.y > 0.0) p = float2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0, 0.0);
    Out = -length(p) * sign(p.y);
}

////Isosceles Triangle - exact(https://www.shadertoy.com/view/MldcD7)
void TriangleIsosceles_float(in float2 p, in float2 q, out float Out)
{
    p.x = abs(p.x);
    float2 a = p - q * clamp(dot(p, q) / dot(q, q), 0.0, 1.0);
    float2 b = p - q * float2(clamp(p.x / q.x, 0.0, 1.0), 1.0);
    float s = -sign(q.y);
    float2 d = min(float2(dot(a, a), s * (p.x * q.y - p.y * q.x)),
        float2(dot(b, b), s * (p.y - q.y)));
    Out = -sqrt(d.x) * sign(d.y);
}

////Triangle - exact(https://www.shadertoy.com/view/XsXSz4)
void Triangle_float(in float2 p, in float2 p0, in float2 p1, in float2 p2, out float Out)
{
    float2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
    float2 v0 = p - p0, v1 = p - p1, v2 = p - p2;
    float2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    float2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    float2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);
    float s = sign(e0.x * e2.y - e0.y * e2.x);
    float2 d = min(min(float2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
        float2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
        float2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
    Out = -sqrt(d.x) * sign(d.y);
}

//Regular Pentagon - exact(https://www.shadertoy.com/view/llVyWW)

void Pentagon_float(in float2 p, in float r, out float Out)
{
    const float3 k = float3(0.809016994, 0.587785252, 0.726542528);
    p.x = abs(p.x);
    p -= 2.0 * min(dot(float2(-k.x, k.y), p), 0.0) * float2(-k.x, k.y);
    p -= 2.0 * min(dot(float2(k.x, k.y), p), 0.0) * float2(k.x, k.y);
    p -= float2(clamp(p.x, -r * k.z, r * k.z), r);
    Out = length(p) * sign(p.y);
}

//Regular Hexagon - exact
void Hexagon_float(in float2 p, in float r, out float Out)
{
    const float3 k = float3(-0.866025404, 0.5, 0.577350269);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= float2(clamp(p.x, -k.z * r, k.z * r), r);
    Out = length(p) * sign(p.y);
}

//Regular Octogon - exact(https://www.shadertoy.com/view/llGfDG)

void Octogon_float(in float2 p, in float r, out float Out)
{
    const float3 k = float3(-0.9238795325, 0.3826834323, 0.4142135623);
    p = abs(p);
    p -= 2.0 * min(dot(float2(k.x, k.y), p), 0.0) * float2(k.x, k.y);
    p -= 2.0 * min(dot(float2(-k.x, k.y), p), 0.0) * float2(-k.x, k.y);
    p -= float2(clamp(p.x, -k.z * r, k.z * r), r);
    Out = length(p) * sign(p.y);
}

//Hexagram - exact(https://www.shadertoy.com/view/tt23RR)

float Hexagram(in float2 p, in float r, out float Out)
{
    const float4 k = float4(-0.5, 0.8660254038, 0.5773502692, 1.7320508076);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= 2.0 * min(dot(k.yx, p), 0.0) * k.yx;
    p -= float2(clamp(p.x, r * k.z, r * k.w), r);
    Out = length(p) * sign(p.y);
}

//Star 5 - exact(https://www.shadertoy.com/view/3tSGDy)

void Star5_float(in float2 p, in float r, in float rf, out float Out)
{
    const float2 k1 = float2(0.809016994375, -0.587785252292);
    const float2 k2 = float2(-k1.x, k1.y);
    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);
    p.y -= r;
    float2 ba = rf * float2(-k1.y, k1.x) - float2(0, 1);
    float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);
    Out = length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}

//Pie - exact(https://www.shadertoy.com/view/3l23RK)

void Pie_float(in float2 p, in float2 c, in float r, out float Out)
{
    p.x = abs(p.x);
    float l = length(p) - r;
    float m = length(p - c * clamp(dot(p, c), 0.0, r));
    Out = max(l, m * sin(c.y * p.x - c.x * p.y));
}

//Cut Disk - exact(https://www.shadertoy.com/view/ftVXRc)

void CutDisk_float(in float2 p, in float r, in float h, out float Out)
{
    float w = sqrt(r * r - h * h); // constant for any given shape
    p.x = abs(p.x);
    float s = max((h - r) * p.x * p.x + w * w * (h + r - 2.0 * p.y), h * p.x - w * p.y);
    Out = (s < 0.0) ? length(p) - r :
        (p.x < w) ? h - p.y :
        length(p - float2(w, h));
}

//Arc - exact(https://www.shadertoy.com/view/wl23RK)

void Arc_float(in float2 p, in float2 sc, in float ra, float rb, out float Out)
{
    // sc is the sin/cos of the arc's aperture
    p.x = abs(p.x);
    Out = ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) :
        abs(length(p) - ra)) - rb;
}

//Moon - exact(https://www.shadertoy.com/view/WtdBRS)

void Moon_float(float2 p, float d, float ra, float rb, out float Out)
{
    p.y = abs(p.y);
    float a = (ra * ra - rb * rb + d * d) / (2.0 * d);
    float b = sqrt(max(ra * ra - a * a, 0.0));
    if (d * (p.x * b - p.y * a) > d * d * max(b - p.y, 0.0))
        Out = length(p - float2(a, b));
    Out = max((length(p) - ra),
        -(length(p - float2(d, 0)) - rb));
}

//Circle Cross - exact(https://www.shadertoy.com/view/NslXDM)

void RoundedCross_float(in float2 p, in float h, in float scale, out float Out)
{
    p /= scale;

    float k = 0.5 * (h + 1.0 / h); // k should be const at modeling time
    p = abs(p);
    Out = (p.x < 1.0 && p.y < p.x* (k - h) + h) ?
        k - sqrt(dot2(p - float2(1, k))) :
        sqrt(min(dot2(p - float2(0, h)),
            dot2(p - float2(1, 0))));
}

//Simple Egg - exact(https://www.shadertoy.com/view/XtVfRW)

void Egg_float(in float2 p, in float ra, in float rb, out float Out)
{
    const float k = sqrt(3.0);
    p.x = abs(p.x);
    float r = ra - rb;
    Out = ((p.y < 0.0) ? length(float2(p.x, p.y)) - r :
        (k * (p.x + r) < p.y) ? length(float2(p.x, p.y - k * r)) :
        length(float2(p.x + r, p.y)) - 2.0 * r) - rb;
}

//Heart - exact(https://www.shadertoy.com/view/3tyBzV)

void Heart_float(in float2 p, in float scale, out float Out)
{
    p /= scale;
    p.y += 0.5;
    p.x = abs(p.x);

    if (p.y + p.x > 1.0)
    {
        Out = sqrt(dot2(p - float2(0.25, 0.75))) - sqrt(2.0) / 4.0;
    }
    else
    {
        Out = sqrt(min(dot2(p - float2(0.00, 1.00)), dot2(p - 0.5 * max(p.x + p.y, 0.0)))) * sign(p.x - p.y);
    }
}

//Cross - exact exterior, bound interior(https://www.shadertoy.com/view/XtGfzw)

void Cross_float(in float2 p, in float2 b, float r, out float Out)
{
    p = abs(p); p = (p.y > p.x) ? p.yx : p.xy;
    float2  q = p - b;
    float k = max(q.y, q.x);
    float2  w = (k > 0.0) ? q : float2(b.y - p.x, -k);
    Out = sign(k) * length(max(w, 0.0)) + r;
}

const int N = 11;
float2 polygons[11];

float cross2d(in float2 v0, in float2 v1) { return v0.x * v1.y - v0.y * v1.x; }

float SDF_Polygon(float2 p, int length)
{
    float d = dot(p - polygons[0], p - polygons[0]);
    float s = 1.0;

    for (int i = 0; i < length; i++)
    {
        // distance
        int i1 = i;
        int i2 = fmod(i + 1, length);
        float2 e = polygons[i2] - polygons[i1];
        float2 v = float2(p - polygons[i1]);
        float2 pq = v - (e * clamp(dot(v, e) / dot(e, e), 0.0, 1.0));
        d = min(d, dot(pq, pq));

        // winding number from http://geomalgorithms.com/a03-_inclusion.html
        // with a bit of help from https://www.shadertoy.com/view/wdBXRW
        float3 cond = float3(p.y >= polygons[i1].y, p.y < polygons[i2].y, e.x* v.y > e.y * v.x);
        if (all(cond) || all(!(cond))) s *= -1.0;  // have a valid up or down intersect 
    }

    return sqrt(d) * s;
}

void Polygon_float(in float2 position, in int count, in float2 p0, in float2 p1, in float2 p2, in float2 p3, in float2 p4, in float2 p5,
    in float2 p6, in float2 p7, in float2 p8, in float2 p9, in float2 p10, out float Out)
{
    polygons[0] = p0;
    polygons[1] = p1;
    polygons[2] = p2;
    polygons[3] = p3;
    polygons[4] = p4;
    polygons[5] = p5;
    polygons[6] = p6;
    polygons[7] = p7;
    polygons[8] = p8;
    polygons[9] = p9;
    polygons[10] = p10;

    Out = SDF_Polygon(position, count);
}


//Parabola - exact(https://www.shadertoy.com/view/ws3GD7)

void Parabola_float(in float2 pos, in float k, out float Out)
{
    pos.x = abs(pos.x);
    float ik = 1.0 / k;
    float p = ik * (pos.y - 0.5 * ik) / 3.0;
    float q = 0.25 * ik * ik * pos.x;
    float h = q * q - p * p * p;
    float r = sqrt(abs(h));
    float x = (h > 0.0) ?
        pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q) :
        2.0 * cos(atan2(r, q) / 3.0) * sqrt(p);
    Out = length(pos - float2(x, k * x * x)) * sign(pos.x - x);
}

//Parabola Segment - exact(https://www.shadertoy.com/view/3lSczz)

void Parabola_Segment_float(in float2 pos, in float wi, in float he, out float Out)
{
    pos.x = abs(pos.x);
    float ik = wi * wi / he;
    float p = ik * (he - pos.y - 0.5 * ik) / 3.0;
    float q = pos.x * ik * ik * 0.25;
    float h = q * q - p * p * p;
    float r = sqrt(abs(h));
    float x = (h > 0.0) ?
        pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q) :
        2.0 * cos(atan(r / q) / 3.0) * sqrt(p);
    x = min(x, wi);
    Out = length(pos - float2(x, he - x * x / ik)) *
        sign(ik * (pos.y - he) + pos.x * pos.x);
}

//Bobbly Cross - exact(https://www.shadertoy.com/view/NssXWM)

void BlobbyCross_float(in float2 pos, float he, out float Out)
{
    pos = abs(pos);
    pos = float2(abs(pos.x - pos.y), 1.0 - pos.x - pos.y) / sqrt(2.0);

    float p = (he - pos.y - 0.25 / he) / (6.0 * he);
    float q = pos.x / (he * he * 16.0);
    float h = q * q - p * p * p;

    float x;
    if (h > 0.0) { float r = sqrt(h); x = pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q); }
    else { float r = sqrt(p); x = 2.0 * r * cos(acos(q / (p * r)) / 3.0); }
    x = min(x, sqrt(2.0) / 2.0);

    float2 z = float2(x, he * (1.0 - 2.0 * x * x)) - pos;
    Out = length(z) * sign(z.y);
}

//Tunnel - exact(https://www.shadertoy.com/view/flSSDy)

void Tunnel_float(in float2 p, in float2 wh, out float Out)
{
    p.x = abs(p.x); p.y = -p.y;
    float2 q = p - wh;

    float d1 = dot2(float2(max(q.x, 0.0), q.y));
    q.x = (p.y > 0.0) ? q.x : length(p) - wh.x;
    float d2 = dot2(float2(q.x, max(q.y, 0.0)));
    float d = sqrt(min(d1, d2));

    Out = (max(q.x, q.y) < 0.0) ? -d : d;
}

void SmoothUnion_float(float d1, float d2, float k, out float Out) 
{
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    Out = lerp(d2, d1, h) - k * h * (1.0 - h);
}

void SmoothSubtraction_float(float d1, float d2, float k, out float Out) 
{
    float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    Out = lerp(d2, -d1, h) + k * h * (1.0 - h);
}

void SmoothIntersection_float(float d1, float d2, float k, out float Out) 
{
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    Out = lerp(d2, d1, h) + k * h * (1.0 - h);
}


//Check Out - https://www.ronja-tutorials.com/
//Sampling
void Lines_float(float dist, float4 _InsideColor, float4 _OutsideColor, float _LineDistance, float _LineThickness, float _SubLines, float _SubLineThickness, out float4 Out)
{
    float4 col = lerp(_InsideColor, _OutsideColor, step(0, dist));

    float distanceChange = fwidth(dist) * 0.5;
    float majorLineDistance = abs(frac(dist / _LineDistance + 0.5) - 0.5) * _LineDistance;
    float majorLines = smoothstep(_LineThickness - distanceChange, _LineThickness + distanceChange, majorLineDistance);

    float distanceBetweenSubLines = _LineDistance / _SubLines;
    float subLineDistance = abs(frac(dist / distanceBetweenSubLines + 0.5) - 0.5) * distanceBetweenSubLines;
    float subLines = smoothstep(_SubLineThickness - distanceChange, _SubLineThickness + distanceChange, subLineDistance);

    Out = col * majorLines * abs(subLines);
}































//Half

//https://iquilezles.org/articles/distfunctions2d/

//Single

half dot2(in half2 v) { return dot(v, v); }
half dot2(in half3 v) { return dot(v, v); }
half ndot(in half2 a, in half2 b) { return a.x * b.x - a.y * b.y; }


//Circle - exact(https://www.shadertoy.com/view/3ltSW2)

void Circle_half(half2 p, half r, out half Out)
{
    Out = length(p) - r;
}

//Rounded Box - exact(https://www.shadertoy.com/view/4llXD7 and https://www.youtube.com/watch?v=s5NGeUV2EyU)

void RoundedBox_half(in half2 p, in half2 b, in half4 r, out half Out)
{
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    half2 q = abs(p) - b + r.x;
    Out = min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

//Box - exact(https://www.youtube.com/watch?v=62-pRVZuS5c)

void Box_half(in half2 p, in half2 b, out half Out)
{
    half2 d = abs(p) - b;
    Out = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

////Segment - exact(https://www.shadertoy.com/view/3tdSDj and https://www.youtube.com/watch?v=PMltMdi1Wzg)

void Segment_half(in half2 p, in half2 a, in half2 b, out half Out)
{
    half2 pa = p - a, ba = b - a;
    half h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    Out = length(pa - ba * h);
}

////Rhombus - exact(https://www.shadertoy.com/view/XdXcRB)

void Rhombus_half(in half2 p, in half2 b, out half Out)
{
    p = abs(p);
    half h = clamp(ndot(b - 2.0 * p, b) / dot(b, b), -1.0, 1.0);
    half d = length(p - 0.5 * b * half2(1.0 - h, 1.0 + h));
    Out = d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
}

////Isosceles Trapezoid - exact(https://www.shadertoy.com/view/MlycD3)

void Trapezoid_half(in half2 p, in half r1, half r2, half he, out half Out)
{
    half2 k1 = half2(r2, he);
    half2 k2 = half2(r2 - r1, 2.0 * he);
    p.x = abs(p.x);
    half2 ca = half2(p.x - min(p.x, (p.y < 0.0) ? r1 : r2), abs(p.y) - he);
    half2 cb = p - k1 + k2 * clamp(dot(k1 - p, k2) / dot2(k2), 0.0, 1.0);
    half s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    Out = s * sqrt(min(dot2(ca), dot2(cb)));
}

////Parallelogram - exact(https://www.shadertoy.com/view/7dlGRf)

void Parallelogram_half(in half2 p, half wi, half he, half sk, out half Out)
{
    half2 e = half2(sk, he);
    p = (p.y < 0.0) ? -p : p;
    half2  w = p - e; w.x -= clamp(w.x, -wi, wi);
    half2  d = half2(dot(w, w), -w.y);
    half s = p.x * e.y - p.y * e.x;
    p = (s < 0.0) ? -p : p;
    half2  v = p - half2(wi, 0); v -= e * clamp(dot(v, e) / dot(e, e), -1.0, 1.0);
    d = min(d, half2(dot(v, v), wi * he - abs(s)));
    Out = sqrt(d.x) * sign(-d.y);
}

//Equilateral Triangle - exact(https://www.shadertoy.com/view/Xl2yDW)

void EquilateralTriangle_half(in half2 p, in half scale, out half Out)
{
    p /= scale;
    const half k = sqrt(3.0);
    p.x = abs(p.x) - 1.0;
    p.y = p.y + 1.0 / k;
    if (p.x + k * p.y > 0.0) p = half2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0, 0.0);
    Out = -length(p) * sign(p.y);
}

////Isosceles Triangle - exact(https://www.shadertoy.com/view/MldcD7)
void TriangleIsosceles_half(in half2 p, in half2 q, out half Out)
{
    p.x = abs(p.x);
    half2 a = p - q * clamp(dot(p, q) / dot(q, q), 0.0, 1.0);
    half2 b = p - q * half2(clamp(p.x / q.x, 0.0, 1.0), 1.0);
    half s = -sign(q.y);
    half2 d = min(half2(dot(a, a), s * (p.x * q.y - p.y * q.x)),
        half2(dot(b, b), s * (p.y - q.y)));
    Out = -sqrt(d.x) * sign(d.y);
}

////Triangle - exact(https://www.shadertoy.com/view/XsXSz4)
void Triangle_half(in half2 p, in half2 p0, in half2 p1, in half2 p2, out half Out)
{
    half2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
    half2 v0 = p - p0, v1 = p - p1, v2 = p - p2;
    half2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    half2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    half2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);
    half s = sign(e0.x * e2.y - e0.y * e2.x);
    half2 d = min(min(half2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
        half2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
        half2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
    Out = -sqrt(d.x) * sign(d.y);
}

//Regular Pentagon - exact(https://www.shadertoy.com/view/llVyWW)

void Pentagon_half(in half2 p, in half r, out half Out)
{
    const half3 k = half3(0.809016994, 0.587785252, 0.726542528);
    p.x = abs(p.x);
    p -= 2.0 * min(dot(half2(-k.x, k.y), p), 0.0) * half2(-k.x, k.y);
    p -= 2.0 * min(dot(half2(k.x, k.y), p), 0.0) * half2(k.x, k.y);
    p -= half2(clamp(p.x, -r * k.z, r * k.z), r);
    Out = length(p) * sign(p.y);
}

//Regular Hexagon - exact
void Hexagon_half(in half2 p, in half r, out half Out)
{
    const half3 k = half3(-0.866025404, 0.5, 0.577350269);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= half2(clamp(p.x, -k.z * r, k.z * r), r);
    Out = length(p) * sign(p.y);
}

//Regular Octogon - exact(https://www.shadertoy.com/view/llGfDG)

void Octogon_half(in half2 p, in half r, out half Out)
{
    const half3 k = half3(-0.9238795325, 0.3826834323, 0.4142135623);
    p = abs(p);
    p -= 2.0 * min(dot(half2(k.x, k.y), p), 0.0) * half2(k.x, k.y);
    p -= 2.0 * min(dot(half2(-k.x, k.y), p), 0.0) * half2(-k.x, k.y);
    p -= half2(clamp(p.x, -k.z * r, k.z * r), r);
    Out = length(p) * sign(p.y);
}

//Hexagram - exact(https://www.shadertoy.com/view/tt23RR)

half Hexagram(in half2 p, in half r, out half Out)
{
    const half4 k = half4(-0.5, 0.8660254038, 0.5773502692, 1.7320508076);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= 2.0 * min(dot(k.yx, p), 0.0) * k.yx;
    p -= half2(clamp(p.x, r * k.z, r * k.w), r);
    Out = length(p) * sign(p.y);
}

//Star 5 - exact(https://www.shadertoy.com/view/3tSGDy)

void Star5_half(in half2 p, in half r, in half rf, out half Out)
{
    const half2 k1 = half2(0.809016994375, -0.587785252292);
    const half2 k2 = half2(-k1.x, k1.y);
    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);
    p.y -= r;
    half2 ba = rf * half2(-k1.y, k1.x) - half2(0, 1);
    half h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);
    Out = length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}

//Pie - exact(https://www.shadertoy.com/view/3l23RK)

void Pie_half(in half2 p, in half2 c, in half r, out half Out)
{
    p.x = abs(p.x);
    half l = length(p) - r;
    half m = length(p - c * clamp(dot(p, c), 0.0, r));
    Out = max(l, m * sin(c.y * p.x - c.x * p.y));
}

//Cut Disk - exact(https://www.shadertoy.com/view/ftVXRc)

void CutDisk_half(in half2 p, in half r, in half h, out half Out)
{
    half w = sqrt(r * r - h * h); // constant for any given shape
    p.x = abs(p.x);
    half s = max((h - r) * p.x * p.x + w * w * (h + r - 2.0 * p.y), h * p.x - w * p.y);
    Out = (s < 0.0) ? length(p) - r :
        (p.x < w) ? h - p.y :
        length(p - half2(w, h));
}

//Arc - exact(https://www.shadertoy.com/view/wl23RK)

void Arc_half(in half2 p, in half2 sc, in half ra, half rb, out half Out)
{
    // sc is the sin/cos of the arc's aperture
    p.x = abs(p.x);
    Out = ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) :
        abs(length(p) - ra)) - rb;
}

//Moon - exact(https://www.shadertoy.com/view/WtdBRS)

void Moon_half(half2 p, half d, half ra, half rb, out half Out)
{
    p.y = abs(p.y);
    half a = (ra * ra - rb * rb + d * d) / (2.0 * d);
    half b = sqrt(max(ra * ra - a * a, 0.0));
    if (d * (p.x * b - p.y * a) > d * d * max(b - p.y, 0.0))
        Out = length(p - half2(a, b));
    Out = max((length(p) - ra),
        -(length(p - half2(d, 0)) - rb));
}

//Circle Cross - exact(https://www.shadertoy.com/view/NslXDM)

void RoundedCross_half(in half2 p, in half h, in half scale, out half Out)
{
    p /= scale;

    half k = 0.5 * (h + 1.0 / h); // k should be const at modeling time
    p = abs(p);
    Out = (p.x < 1.0 && p.y < p.x* (k - h) + h) ?
        k - sqrt(dot2(p - half2(1, k))) :
        sqrt(min(dot2(p - half2(0, h)),
            dot2(p - half2(1, 0))));
}

//Simple Egg - exact(https://www.shadertoy.com/view/XtVfRW)

void Egg_half(in half2 p, in half ra, in half rb, out half Out)
{
    const half k = sqrt(3.0);
    p.x = abs(p.x);
    half r = ra - rb;
    Out = ((p.y < 0.0) ? length(half2(p.x, p.y)) - r :
        (k * (p.x + r) < p.y) ? length(half2(p.x, p.y - k * r)) :
        length(half2(p.x + r, p.y)) - 2.0 * r) - rb;
}

//Heart - exact(https://www.shadertoy.com/view/3tyBzV)

void Heart_half(in half2 p, in half scale, out half Out)
{
    p /= scale;
    p.y += 0.5;
    p.x = abs(p.x);

    if (p.y + p.x > 1.0)
    {
        Out = sqrt(dot2(p - half2(0.25, 0.75))) - sqrt(2.0) / 4.0;
    }
    else
    {
        Out = sqrt(min(dot2(p - half2(0.00, 1.00)), dot2(p - 0.5 * max(p.x + p.y, 0.0)))) * sign(p.x - p.y);
    }
}

//Cross - exact exterior, bound interior(https://www.shadertoy.com/view/XtGfzw)

void Cross_half(in half2 p, in half2 b, half r, out half Out)
{
    p = abs(p); p = (p.y > p.x) ? p.yx : p.xy;
    half2  q = p - b;
    half k = max(q.y, q.x);
    half2  w = (k > 0.0) ? q : half2(b.y - p.x, -k);
    Out = sign(k) * length(max(w, 0.0)) + r;
}


half SDF_Polygon(half2 p, int length)
{
    half d = dot(p - polygons[0], p - polygons[0]);
    half s = 1.0;

    for (int i = 0; i < length; i++)
    {
        // distance
        int i1 = i;
        int i2 = fmod(i + 1, length);
        half2 e = polygons[i2] - polygons[i1];
        half2 v = half2(p - polygons[i1]);
        half2 pq = v - (e * clamp(dot(v, e) / dot(e, e), 0.0, 1.0));
        d = min(d, dot(pq, pq));

        // winding number from http://geomalgorithms.com/a03-_inclusion.html
        // with a bit of help from https://www.shadertoy.com/view/wdBXRW
        half3 cond = half3(p.y >= polygons[i1].y, p.y < polygons[i2].y, e.x* v.y > e.y * v.x);
        if (all(cond) || all(!(cond))) s *= -1.0;  // have a valid up or down intersect 
    }

    return sqrt(d) * s;
}

void Polygon_half(in half2 position, in int count, in half2 p0, in half2 p1, in half2 p2, in half2 p3, in half2 p4, in half2 p5,
    in half2 p6, in half2 p7, in half2 p8, in half2 p9, in half2 p10, out half Out)
{
    polygons[0] = p0;
    polygons[1] = p1;
    polygons[2] = p2;
    polygons[3] = p3;
    polygons[4] = p4;
    polygons[5] = p5;
    polygons[6] = p6;
    polygons[7] = p7;
    polygons[8] = p8;
    polygons[9] = p9;
    polygons[10] = p10;

    Out = SDF_Polygon(position, count);
}


//Parabola - exact(https://www.shadertoy.com/view/ws3GD7)

void Parabola_half(in half2 pos, in half k, out half Out)
{
    pos.x = abs(pos.x);
    half ik = 1.0 / k;
    half p = ik * (pos.y - 0.5 * ik) / 3.0;
    half q = 0.25 * ik * ik * pos.x;
    half h = q * q - p * p * p;
    half r = sqrt(abs(h));
    half x = (h > 0.0) ?
        pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q) :
        2.0 * cos(atan2(r, q) / 3.0) * sqrt(p);
    Out = length(pos - half2(x, k * x * x)) * sign(pos.x - x);
}

//Parabola Segment - exact(https://www.shadertoy.com/view/3lSczz)

void Parabola_Segment_half(in half2 pos, in half wi, in half he, out half Out)
{
    pos.x = abs(pos.x);
    half ik = wi * wi / he;
    half p = ik * (he - pos.y - 0.5 * ik) / 3.0;
    half q = pos.x * ik * ik * 0.25;
    half h = q * q - p * p * p;
    half r = sqrt(abs(h));
    half x = (h > 0.0) ?
        pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q) :
        2.0 * cos(atan(r / q) / 3.0) * sqrt(p);
    x = min(x, wi);
    Out = length(pos - half2(x, he - x * x / ik)) *
        sign(ik * (pos.y - he) + pos.x * pos.x);
}

//Bobbly Cross - exact(https://www.shadertoy.com/view/NssXWM)

void BlobbyCross_half(in half2 pos, half he, out half Out)
{
    pos = abs(pos);
    pos = half2(abs(pos.x - pos.y), 1.0 - pos.x - pos.y) / sqrt(2.0);

    half p = (he - pos.y - 0.25 / he) / (6.0 * he);
    half q = pos.x / (he * he * 16.0);
    half h = q * q - p * p * p;

    half x;
    if (h > 0.0) { half r = sqrt(h); x = pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q); }
    else { half r = sqrt(p); x = 2.0 * r * cos(acos(q / (p * r)) / 3.0); }
    x = min(x, sqrt(2.0) / 2.0);

    half2 z = half2(x, he * (1.0 - 2.0 * x * x)) - pos;
    Out = length(z) * sign(z.y);
}

//Tunnel - exact(https://www.shadertoy.com/view/flSSDy)

void Tunnel_half(in half2 p, in half2 wh, out half Out)
{
    p.x = abs(p.x); p.y = -p.y;
    half2 q = p - wh;

    half d1 = dot2(half2(max(q.x, 0.0), q.y));
    q.x = (p.y > 0.0) ? q.x : length(p) - wh.x;
    half d2 = dot2(half2(q.x, max(q.y, 0.0)));
    half d = sqrt(min(d1, d2));

    Out = (max(q.x, q.y) < 0.0) ? -d : d;
}

void SmoothUnion_half(half d1, half d2, half k, out half Out)
{
    half h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    Out = lerp(d2, d1, h) - k * h * (1.0 - h);
}

void SmoothSubtraction_half(half d1, half d2, half k, out half Out)
{
    half h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    Out = lerp(d2, -d1, h) + k * h * (1.0 - h);
}

void SmoothIntersection_half(half d1, half d2, half k, out half Out)
{
    half h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    Out = lerp(d2, d1, h) + k * h * (1.0 - h);
}


//Check Out - https://www.ronja-tutorials.com/
//Sampling
void Lines_half(half dist, half4 _InsideColor, half4 _OutsideColor, half _LineDistance, half _LineThickness, half _SubLines, half _SubLineThickness, out half4 Out)
{
    half4 col = lerp(_InsideColor, _OutsideColor, step(0, dist));

    half distanceChange = fwidth(dist) * 0.5;
    half majorLineDistance = abs(frac(dist / _LineDistance + 0.5) - 0.5) * _LineDistance;
    half majorLines = smoothstep(_LineThickness - distanceChange, _LineThickness + distanceChange, majorLineDistance);

    half distanceBetweenSubLines = _LineDistance / _SubLines;
    half subLineDistance = abs(frac(dist / distanceBetweenSubLines + 0.5) - 0.5) * distanceBetweenSubLines;
    half subLines = smoothstep(_SubLineThickness - distanceChange, _SubLineThickness + distanceChange, subLineDistance);

    Out = col * majorLines * abs(subLines);
}












