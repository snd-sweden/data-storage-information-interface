<?php
namespace OCA\RoCrateGenerator\Controller;

use OCP\IRequest;
use OCP\AppFramework\Controller;
use OCP\AppFramework\Http;
use OCP\AppFramework\Http\DataResponse;
use OCP\Files\Folder;
use OCP\IUserManager;
use GuzzleHttp;
use OCP\IConfig;

class RoCrateController extends Controller {

	private $userFolder;
	private $user;
	private $config;

	public function __construct($AppName, IRequest $request, Folder $userFolder, IUserManager $userManager, IConfig $config, $UserId){
		parent::__construct($AppName, $request);
		$this->user = $userManager->get($UserId);
		$this->userFolder = $userFolder;
		$this->config = $config;
	}

	/**
	* CAUTION: the @Stuff turns off security checks; for this page no admin is
	*          required and no CSRF check. If you don't know what CSRF is, read
	*          it up in the docs or you might create a security hole. This is
	*          basically the only required method to add this exemption, don't
	*          add it to any other method if you don't exactly know what it does
	*
	* @NoAdminRequired
	* @NoCSRFRequired
	*/
	public function index($path) {


		$folder = $this->userFolder->get($path);
		if ($folder->getType() !== Folder::TYPE_FOLDER) {
			return new DataResponse([], Http::STATUS_BAD_REQUEST);
		}

		$publisherDomain = $this->config->getAppValue('rocrategenerator', 'publisherDomain', false);

		if (!$publisherDomain) {
			return new DataResponse([], Http::STATUS_BAD_REQUEST);
		}

		//TODO: Load and update existing rocrate manifest if file exists

		$fileIds = [];
		$graph = [
			[
				"@type" => "CreativeWork",
				"@id" => "ro-crate-metadata.json",
				"identifier" => $this->generateUUID(), //TODO: Don't generate a new UUID everytime
				"conformsTo" => [
					"@id" => "https://w3id.org/ro/crate/1.1"
				],
				"about" => [
					"@id" => "./"
				],
				"publisher" => [
					"@id" => "#myPublisherId" //TODO: Use ROR-ID
				],
				"creator" => [
					[
						"@id" => "#person-0", //TODO: use orcid
					]
				]
			],
			[
				"@type" => "Organization",
				"@id" => "#myPublisherId", //TODO: Use ROR-ID
				"identifier" => [
					[
						"@id" => "#domain-0"
					]
				]
			],
			[
				"@type" => "PropertyValue",
				"@id" => "#domain-0",
				"propertyID" => "domain",
				"value" => $publisherDomain
			]
		];

		$graph[] = [
			"@type" => "Person",
			"@id" => "#person-0", //TODO: use orcid
			"identifier" => [
				[
					"@id" => "#eduPersonPrincipalName-0",
				]
			]
		];

		$graph[] = [
			"@type" => "PropertyValue",
			"@id" => "#eduPersonPrincipalName-0",
			"propertyID" => "eduPersonPrincipalName",
			"value" => $this->user->getUID(), //TODO: user attribute swamid
		];

		$files = $this->walk($folder);
		foreach ($files as $file) {
			$relPath = str_replace($folder->getInternalPath() . '/', '', $file->getInternalPath());
			$stat = $file->stat();

			$graph[] = [
				"@type" => "File",
				"@id" => $relPath,
				"sha256" => $file->hash("sha256"), //TODO: only calculate hash for small files
				"contentSize" => $file->getSize(),
				"dateCreated" => gmdate("Y-m-d\TH:i:s\Z", $stat["ctime"]),
				"dateModified" => gmdate("Y-m-d\TH:i:s\Z", $stat["mtime"]),
				"encodingFormat" => $file->getMimeType(),
				//TODO: url if files is public
			];

			$fileIds[] = $relPath;
		}

		$graph[] = [
			"@type" => "Dataset",
			"@id" => "./",
			"name" => $folder->getName(),
			"hasPart" => $fileIds,
		];

		$roCrate = [
			"@context" => "https://w3id.org/ro/crate/1.1/context",
			"@graph" => $graph
		];

		$roCrateJson = json_encode($roCrate, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);

		$saveManifestInFolder = $this->config->getAppValue('rocrategenerator', 'saveManifestInFolder', false);

		if ($saveManifestInFolder) {
			$roCrateFile = $folder->newFile('ro-crate-metadata.json');
			$roCrateFile->putContent($roCrateJson);
		}

		$url = $this->config->getAppValue('rocrategenerator', 'postManifestToUrl', false);
		if ($url && filter_var($url, FILTER_VALIDATE_URL)) {
			$client = new GuzzleHttp\Client();
			$response = $client->post($url, [GuzzleHttp\RequestOptions::JSON => $roCrate]); //TODO: do something with response?
		}

		return new DataResponse($roCrate);
	}

	private function walk($folder, $fileList = [])
	{
		foreach($folder->getDirectoryListing() as $node) {
			if ($node->getType() == Folder::TYPE_FOLDER) {
				$fileList += $this->walk($node, $fileList);
			} elseif ($node->getName() !== "ro-crate-metadata.json") {
				$fileList[] = $node;
			}
		}

		return $fileList;
	}

	private function generateUUID() {
		return sprintf( '%04x%04x-%04x-%04x-%04x-%04x%04x%04x',
		// 32 bits for "time_low"
		mt_rand( 0, 0xffff ), mt_rand( 0, 0xffff ),

		// 16 bits for "time_mid"
		mt_rand( 0, 0xffff ),

		// 16 bits for "time_hi_and_version",
		// four most significant bits holds version number 4
		mt_rand( 0, 0x0fff ) | 0x4000,

		// 16 bits, 8 bits for "clk_seq_hi_res",
		// 8 bits for "clk_seq_low",
		// two most significant bits holds zero and one for variant DCE1.1
		mt_rand( 0, 0x3fff ) | 0x8000,

		// 48 bits for "node"
		mt_rand( 0, 0xffff ), mt_rand( 0, 0xffff ), mt_rand( 0, 0xffff )
);
	}
}
