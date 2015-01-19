#version 330

uniform float Time;
uniform mat4 Matrix;
uniform mat4 ModelMatrix;
uniform mat4 NormMatrix;

layout(triangles) in;
layout(triangle_strip, max_vertices = 9) out;

in vec3 vert_pos[];
in vec3 vert_norm[];
in vec2 vert_uv[];

in vec2 geom_uv[];
in vec3 geom_vertex[];
in vec3 geom_vertex_raw[];
in vec3 geom_normal[];

out vec2 frag_uv;
out vec3 frag_vertex;
out vec3 frag_vertex_raw;
out vec3 frag_normal;

vec3 project(mat4 M, vec3 V) {
	return (M * vec4(V, 1.0)).xyz;
}

vec4 Pos(int I) {
	return gl_in[I].gl_Position;
}

void main() {
	int i;
	vec4 vertex;

	vec4 V = Pos(1) - Pos(0);
	vec4 W = Pos(2) - Pos(0);
	vec3 Norm = -normalize(vec3((V.y * W.z) - (V.z * W.y), (V.z * W.x) - (V.x * W.z), (V.x * W.y) - (V.y * W.x)));

	for(i = 0; i < gl_VerticesIn; i++) {
		vec4 Pos = Pos(i);

		frag_uv = geom_uv[i];
		frag_vertex = geom_vertex[i];
		frag_vertex_raw = geom_vertex_raw[i];
		frag_normal = geom_normal[i];

		float s = clamp(sin(Time * 0.8) * 4, 0.0, 3.0) * 5;
		gl_Position = vec4(Pos.xyz + Norm * s, Pos.w);
		EmitVertex();
	}
	EndPrimitive();
}