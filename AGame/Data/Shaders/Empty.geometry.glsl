#version 150

uniform float Time;
uniform mat4 Matrix;
uniform mat4 ModelMatrix;
uniform mat4 NormMatrix;

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

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

#define MAP(a, b) a = b[i]

void main() {
	for(int i = 0; i < gl_VerticesIn; i++) {
		MAP(frag_uv, geom_uv);
		MAP(frag_vertex, geom_vertex);
		MAP(frag_vertex_raw, geom_vertex_raw);
		MAP(frag_normal, geom_normal);
		gl_Position = gl_in[i].gl_Position;
		EmitVertex();
	}
	EndPrimitive();
}