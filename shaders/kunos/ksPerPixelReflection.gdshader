shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley;

uniform vec4 albedo : source_color = vec4(1,1,1,1);
uniform sampler2D txDiffuse : source_color, filter_linear_mipmap, repeat_enable;

void fragment() {
	vec2 base_uv = UV;

	vec4 albedo_tex = texture(txDiffuse, base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	//ALBEDO = albedo_tex.rgb;

	//float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
	//METALLIC = metallic_tex * metallic;
	//SPECULAR = specular;
	
	//vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
	//float roughness_tex = dot(texture(texture_roughness, base_uv), roughness_texture_channel);
	//ROUGHNESS = roughness_tex * roughness;
	
	//NORMAL_MAP = texture(texture_normal, base_uv).rgb;
	//NORMAL_MAP_DEPTH = normal_scale;
	
	ALPHA *= albedo.a * albedo_tex.a;
	//ALPHA_SCISSOR_THRESHOLD = 0.5;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
