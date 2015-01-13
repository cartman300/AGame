#version 130

uniform mat4 Matrix;

in vec2 vert_pos;
in vec2 vert_uv;

out vec2 frag_uv;

void main() {
	frag_uv = vert_uv;

	gl_Position = Matrix * vec4(vert_pos, 0.0, 1.0);
}