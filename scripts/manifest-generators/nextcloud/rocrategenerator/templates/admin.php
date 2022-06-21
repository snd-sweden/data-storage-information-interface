<?php
/** @var array $_ */

/** @var \OCP\IL10N $l */
script('rocrategenerator', 'settings');

?>
<div id="roCrateSettings" class="section">
	<h2><?php p($l->t('RO-Crate Generator')); ?></h2>
	<p class="settings-hint"><?php p($l->t('Lorem ipsum about RO-Crate')); ?></p>

	<p>
		<input id="saveManifestInFolder" type="checkbox" class="checkbox" <?php if ($_['saveManifestInFolder']) {
	p('checked');
} ?> />
		<label for="saveManifestInFolder"><?php p($l->t('Save generated manifest in the folder')); ?></label><br/>

		<input id="postManifestToUrl" type="input" value="<?php p($_['postManifestToUrl'])?>" />
    <label for="postManifestToUrl"><?php p($l->t('Url to post manifests to')); ?></label><br />

		<input id="publisherDomain" type="input" value="<?php p($_['publisherDomain'])?>" />
    <label for="publisherDomain"><?php p($l->t('Publisher domain')); ?></label><br />

	</p>

</div>