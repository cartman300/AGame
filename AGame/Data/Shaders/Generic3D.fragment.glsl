#version 150

uniform sampler2D tex;

in vec2 frag_uv;
in vec3 V;
in vec3 N;

out vec4 Color;

void main() {
	vec3 LightPos = vec3(0, 100, 0);
	vec3 L = normalize(LightPos - V);
	vec3 E = normalize(-V);
	vec3 R = normalize(-reflect(L, N));

	vec4 Amb = vec4(0.4, 0.4, 0.4, 1.0);
	vec4 Diff = clamp(vec4(0.2, 0.2, 0.2, 1.0) * max(dot(N, L), 0.0), 0.0, 1.0);
	vec4 Spec = clamp(vec4(0.2, 0.2, 0.2, 1.0) * pow(max(dot(R, E), 0.0), 0.3 * 1.8), 0.0, 1.0);

	Color = vec4(0.0, 0.0, 0.0, 1.0) + (texture(tex, frag_uv)) * (Amb + Diff + Spec);
}