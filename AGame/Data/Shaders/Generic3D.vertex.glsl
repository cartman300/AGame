#version 150

uniform mat4 Matrix;

in vec3 vert_pos;
in vec3 vert_norm;
in vec2 vert_uv;

out vec2 frag_uv;

out vec3 V;
out vec3 N;

void main() {
	mat4 NormMatrix = transpose(inverse(Matrix));

	V = (Matrix * vec4(vert_pos, 1.0)).xyz;
	N = normalize(NormMatrix * vec4(vert_norm, 1.0)).xyz;

	frag_uv = vert_uv;

	gl_Position = Matrix * vec4(vert_pos, 1.0);
}