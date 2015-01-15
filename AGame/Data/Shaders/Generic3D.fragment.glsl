#version 150

uniform mat4 Matrix;
uniform sampler2D tex;
uniform int MultColor;
uniform vec4 ObjColor;

in vec2 frag_uv;
in vec3 V;
in vec3 N;

out vec4 Color;

void main() {
	vec3 LightPos = (Matrix * vec4(50, 50, 0, 1)).xyz;
	vec3 L = normalize(LightPos - V);
	vec3 E = normalize(-V);
	vec3 R = normalize(-reflect(L, N));

	vec4 Amb = vec4(0.1, 0.1, 0.1, 1.0);
	vec4 Diff = clamp(vec4(0.2, 0.2, 0.2, 1.0) * max(dot(N, L), 0.0), 0.5, 1.0);
	vec4 Spec = clamp(vec4(0.2, 0.2, 0.2, 1.0) * pow(max(dot(R, E), 0.0), 0.5), 0.0, 1.0);

	vec4 _Color = vec4(0.0, 0.0, 0.0, 1.0) + (texture(tex, frag_uv)) * (Amb + Diff + Spec);
	if (MultColor == 0)
		Color = _Color + ObjColor;
	else
		Color = _Color * ObjColor;
}