shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley;

uniform float ksAmbient = 1.0;
uniform float ksDiffuse = 1.0;
uniform float ksSpecular = 1.0;
uniform float ksSpecularEXP = 0.0;
uniform float ksEmissive = 0.0;
uniform float ksAlphaRef = 0.0;
uniform float multR = 1.0;
uniform float multG = 1.0;
uniform float multB = 1.0;
uniform vec2 multA = vec2(1.0,1.0);
uniform float magicMult = 1.0;

uniform sampler2D txMask : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D txDiffuse : source_color, filter_linear_mipmap_anisotropic, repeat_enable;

uniform sampler2D txDetailR : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D txDetailG : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D txDetailB : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D txDetailA : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D txDetailNM : source_color, filter_linear_mipmap_anisotropic, repeat_enable;

varying vec3 uv1_triplanar_pos;

void vertex() {
	vec3 normal = NORMAL;

	TANGENT = vec3(0.0, 0.0, -1.0) * abs(normal.x);
	TANGENT += vec3(1.0, 0.0, 0.0) * abs(normal.y);
	TANGENT += vec3(1.0, 0.0, 0.0) * abs(normal.z);
	TANGENT = normalize(TANGENT);

	BINORMAL = vec3(0.0, 1.0, 0.0) * abs(normal.x);
	BINORMAL += vec3(0.0, 0.0, -1.0) * abs(normal.y);
	BINORMAL += vec3(0.0, 1.0, 0.0) * abs(normal.z);
	BINORMAL = normalize(BINORMAL);

	// UV1 Triplanar: Enabled
	uv1_triplanar_pos = VERTEX;
	//uv1_triplanar_pos *= vec3(1.0, -1.0, 1.0);
}

vec4 triplanar_texture(sampler2D p_sampler, vec3 p_triplanar_pos, float scale) {
	return texture(p_sampler,p_triplanar_pos.xz*scale);
}

void fragment() {
	vec4 upos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, FRAGCOORD.z, 1.0);
	vec3 pixel_position = upos.xyz / upos.w;
	float dist  = pow((-pixel_position.z) / 5.0,1.0);
	if( dist > 1.0 )
		dist = 1.0;
	
	vec2 base_uv = UV;
	
	vec4 mask = texture(txMask,base_uv);
	vec4 diffuse = texture(txDiffuse,base_uv);
	
	vec3 color = diffuse.rgb;// * ksDiffuse;

	vec4 txr = triplanar_texture(txDetailR,uv1_triplanar_pos,multR);
	vec4 txg = triplanar_texture(txDetailG,uv1_triplanar_pos,multG);
	vec4 txb = triplanar_texture(txDetailB,uv1_triplanar_pos,multB);
	vec4 txa = triplanar_texture(txDetailA,uv1_triplanar_pos,multA.x);

	//color = mix(color,mix(color,tx.rgb,mask.r),.5*(1.0-dist));
	color *= txr.rgb*mask.r + txg.rgb*mask.g + txb.rgb*mask.b + txa.rgb*mask.a;
	color *= magicMult;
	//color *= mix(color,txr.rgb*mask.r + txg.rgb*mask.g + txb.rgb*mask.b + txa.rgb*mask.a,.5*(1.0-dist));
	
	//tx = triplanar_texture(txDetailG,uv1_triplanar_pos,multG/uv1_scale.x);
	//color = mix(color,mix(color,tx.rgb,mask.g),.5*(1.0-dist));

	//tx = triplanar_texture(txDetailB,uv1_triplanar_pos,multB/uv1_scale.x);
	//color = mix(color,mix(color,tx.rgb,mask.b),.5*(1.0-dist));

	//tx = triplanar_texture(txDetailA,uv1_triplanar_pos,uv1_scale.x*20.0);
	//color = mix(diffuse.rgb,color,1.0-dist);
	//color = diffuse.rgb * color;

	ALBEDO = color;
	//ALBEDO = diffuse.rgb;

	//float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
	//METALLIC = metallic_tex * metallic;
	SPECULAR = .5;
	
	//vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
	//float roughness_tex = dot(texture(texture_roughness, base_uv), roughness_texture_channel);
	ROUGHNESS = .5;
	
	vec4 tx = triplanar_texture(txDetailNM,uv1_triplanar_pos,1.0);
	NORMAL_MAP = tx.rgb;
	NORMAL_MAP_DEPTH = .5;
	
	//ALPHA = diffuse.a;
	//ALPHA_SCISSOR_THRESHOLD = 0.1;
}

void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
	vec3 V = VIEW;
	vec3 L = LIGHT;
	vec3 T = NORMAL;

	float shininess = .500;
	//float spec = pow(1.0-abs(dot(V*L,T)),shininess) / shininess * 2.0;
	//spec = 1.0-abs(dot(T,L));
	
	float spec = pow(abs(dot(V*L,T)),.50) * -10.0 + 10.0;

	DIFFUSE_LIGHT += vec3(ksDiffuse,ksDiffuse,ksDiffuse) * ATTENUATION;
	
	float cosTheta = abs(dot(LIGHT*NORMAL,VIEW));

	// Invert and calculate Schlick's Fresnel
	float fresnel = pow(cosTheta, 50.0) * 10.0;
	
	//fresnel *= fresnel;
	
	//SPECULAR_LIGHT += fresnel * LIGHT_COLOR * ATTENUATION * 0.005;
}
