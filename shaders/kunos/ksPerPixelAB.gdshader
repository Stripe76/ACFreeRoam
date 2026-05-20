shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley;

uniform sampler2D txDiffuse : source_color, filter_linear_mipmap, repeat_enable;

// Utilizzato dalle "fences", meglio metterlo AT?

void fragment() {
	vec2 base_uv = UV;

	vec4 diffuse = texture(txDiffuse,base_uv);
	ALBEDO = diffuse.rgb;

	//METALLIC = metallic_tex * metallic;
	//SPECULAR = specular;
	
	//ROUGHNESS = roughness_tex * roughness;
	
	//NORMAL_MAP = texture(texture_normal, base_uv).rgb;
	//NORMAL_MAP_DEPTH = normal_scale;
	
	ALPHA = diffuse.a;
	//ALPHA_SCISSOR_THRESHOLD = 0.5;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
