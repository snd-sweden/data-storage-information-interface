<?php
namespace OCA\RoCrateGenerator\Settings;

use OCP\AppFramework\Http\TemplateResponse;
use OCP\Settings\ISettings;
use OCP\IConfig;

class Admin implements ISettings {

	/** @var IConfig */
	private $config;

	public function __construct(IConfig $config) {
		$this->config = $config;
	}

	public function getForm(): TemplateResponse {

		return new TemplateResponse('rocrategenerator', 'admin', [
			'saveManifestInFolder' => $this->config->getAppValue('rocrategenerator', 'saveManifestInFolder', false),
			'postManifestToUrl' => $this->config->getAppValue('rocrategenerator', 'postManifestToUrl', ""),
			'publisherDomain' => $this->config->getAppValue('rocrategenerator', 'publisherDomain', ""),
			],
			''
		);
	}

	public function getSection(): string {
		return 'sharing';
	}

	public function getPriority(): int {
		return 100;
	}
}
