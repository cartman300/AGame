#version 150

uniform mat4 Matrix;
uniform mat4 ModelMatrix;
uniform mat4 NormMatrix;

uniform sampler2D tex;
uniform int MultColor;
uniform vec4 ObjColor;

in vec2 frag_uv;
in vec3 frag_vertex;
in vec3 frag_vertex_raw;
in vec3 frag_normal;

out vec4 Color;

vec3 project(mat4 M, vec3 V) {
	return (M * vec4(V, 1.0)).xyz;
}

void main() {
	vec4 Ambient = vec4(0.2, 0.2, 0.2, 1.0);
	vec3 LightPos = vec3(0, 1, 0);

	float LStren = max(dot(frag_normal, LightPos), 0.0);
	vec4 TexClr = texture(tex, frag_uv);
	TexClr *= max(Ambient, vec4(LStren, LStren, LStren, 1.0));

	if (MultColor == 0)
		Color = (TexClr + ObjColor);
	else
		Color = (TexClr * ObjColor);
}