﻿using Genesis.Common;
using UnityEditor;

namespace JCMG.Genesis.Editor
{
	internal sealed class AssembliesConfigDrawer : AbstractSettingsDrawer
	{
		/// <summary>
		/// The display title for this drawer
		/// </summary>
		public override string Title => TITLE;

		/// <summary>
		/// The ascending order in which this drawer should be shown with regards to other <see cref="ISettingsDrawer"/>
		/// instances
		/// </summary>
		public override int Order => 1;

		private readonly AssembliesConfig _config;

		// UI
		private const string TITLE = "Assemblies";
		private const string DO_USE_WHITE_LIST_LABEL = "Do Use Whitelist";
		private const string DO_USE_WHITE_LIST_DESCRIPTION = "If enabled, searching via reflection for Data Providers " +
		                                                     "will be limited to the array of assemblies below. Otherwise " +
		                                                     "all loaded assemblies will be searched.";

		private const string ASSEMBLY_WHITE_LIST_LABEL = "Assembly Whitelist";
		private const string ASSEMBLY_WHITE_LIST_DESCRIPTION = "The comma delimited array of assemblies that searching " +
		                                                       "via reflection for Data Providers should be limited to.";

		public AssembliesConfigDrawer()
		{
			_config = new AssembliesConfig();
		}

		/// <summary>Initializes any setup for the drawer prior to rendering any GUI.</summary>
		/// <param name="settings"></param>
		public override void Initialize(GenesisSettings settings)
		{
			base.Initialize(settings);

			_config.Configure(settings);
		}

		protected override void DrawContentBody(GenesisSettings settings)
		{
			// Do Use white-list
			EditorGUILayout.HelpBox(DO_USE_WHITE_LIST_DESCRIPTION, MessageType.Info);
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(DO_USE_WHITE_LIST_LABEL);

				using (var scope = new EditorGUI.ChangeCheckScope())
				{
					var newValue = EditorGUILayout.Toggle(_config.DoUseWhitelistOfAssemblies);

					if (scope.changed)
					{
						_config.DoUseWhitelistOfAssemblies = newValue;

						EditorUtility.SetDirty(settings);
					}
				}
			}

			// White-Listed Assemblies
			EditorGUILayout.HelpBox(ASSEMBLY_WHITE_LIST_DESCRIPTION, MessageType.Info);
			using (new EditorGUI.DisabledScope(!_config.DoUseWhitelistOfAssemblies))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField(ASSEMBLY_WHITE_LIST_LABEL);

					using (var scope = new EditorGUI.ChangeCheckScope())
					{
						var newValue = EditorGUILayout.TextField(_config.RawWhiteListedAssemblies);

						if (scope.changed)
						{
							_config.RawWhiteListedAssemblies = newValue;

							EditorUtility.SetDirty(settings);
						}
					}
				}
			}
		}
	}
}
