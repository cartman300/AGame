#version 150

uniform mat4 Matrix;
uniform mat4 ModelMatrix;
uniform mat4 NormMatrix;

in vec3 vert_pos;
in vec3 vert_norm;
in vec2 vert_uv;

out vec2 geom_uv;
out vec3 geom_vertex;
out vec3 geom_vertex_raw;
out vec3 geom_normal;

vec3 project(mat4 M, vec3 V) {
	return (M * vec4(V, 1.0)).xyz;
}

void main() {
	vec3 normal = project(NormMatrix, vert_norm);
	vec4 vert = Matrix * vec4(vert_pos, 1.0);

	geom_uv = vert_uv;
	geom_vertex = vert.xyz;
	geom_vertex_raw = vert_pos;
	geom_normal = normal;
	gl_Position = vert;
}